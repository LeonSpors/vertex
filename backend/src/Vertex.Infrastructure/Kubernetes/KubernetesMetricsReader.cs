using System.Globalization;
using System.Text.Json;
using k8s;
using k8s.Autorest;
using k8s.Models;
using Microsoft.Extensions.Logging;

namespace Vertex.Infrastructure.Kubernetes;

public sealed record ClusterResourceMetrics(double? CpuUsagePercent, double? MemoryUsagePercent, IReadOnlyDictionary<string, NodeResourceMetrics> Nodes);
public sealed record NodeResourceMetrics(double? CpuUsagePercent, double? MemoryUsagePercent);

/// <summary>
/// Reads the Kubernetes metrics.k8s.io API when metrics-server is installed.
/// Missing metrics are represented as null rather than synthetic values.
/// </summary>
public sealed class KubernetesMetricsReader(ILogger<KubernetesMetricsReader> logger, KubernetesQuantityParser quantityParser)
{
    public async Task<ClusterResourceMetrics> ReadAsync(IKubernetes client, IReadOnlyList<V1Node> nodes, CancellationToken cancellationToken)
    {
        try
        {
            var response = await client.CustomObjects.ListClusterCustomObjectAsync("metrics.k8s.io", "v1beta1", "nodes", cancellationToken: cancellationToken);
            using var document = JsonDocument.Parse(JsonSerializer.Serialize(response));
            var usages = new Dictionary<string, NodeResourceMetrics>(StringComparer.Ordinal);
            double cpuUsage = 0;
            double memoryUsage = 0;

            foreach (var item in document.RootElement.GetProperty("items").EnumerateArray())
            {
                var name = item.GetProperty("metadata").GetProperty("name").GetString();
                var usage = item.GetProperty("usage");
                var cpu = quantityParser.ParseCpu(usage.GetProperty("cpu").GetString());
                var memory = quantityParser.ParseMemory(usage.GetProperty("memory").GetString());
                if (name is null || cpu is null || memory is null) continue;

                cpuUsage += cpu.Value;
                memoryUsage += memory.Value;
                var node = nodes.FirstOrDefault(x => x.Metadata?.Name == name);
                var cpuCapacity = quantityParser.ParseCpu(GetQuantity(node?.Status?.Capacity, "cpu"));
                var memoryCapacity = quantityParser.ParseMemory(GetQuantity(node?.Status?.Capacity, "memory"));
                usages[name] = new NodeResourceMetrics(
                    cpuCapacity > 0 ? cpu.Value / cpuCapacity.Value * 100 : null,
                    memoryCapacity > 0 ? memory.Value / memoryCapacity.Value * 100 : null);
            }

            var totalCpuCapacity = nodes.Sum(x => quantityParser.ParseCpu(GetQuantity(x.Status?.Capacity, "cpu")) ?? 0);
            var totalMemoryCapacity = nodes.Sum(x => quantityParser.ParseMemory(GetQuantity(x.Status?.Capacity, "memory")) ?? 0);
            return new ClusterResourceMetrics(
                totalCpuCapacity > 0 ? cpuUsage / totalCpuCapacity * 100 : null,
                totalMemoryCapacity > 0 ? memoryUsage / totalMemoryCapacity * 100 : null,
                usages);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning("Kubernetes metrics are unavailable. Install metrics-server for live utilization: {Reason}", exception.Message);
            return new ClusterResourceMetrics(null, null, new Dictionary<string, NodeResourceMetrics>());
        }
    }

    private static string? GetQuantity(IDictionary<string, ResourceQuantity>? quantities, string name)
        => quantities is not null && quantities.TryGetValue(name, out var value) ? value.ToString() : null;
}

public sealed class KubernetesQuantityParser
{
    public double? ParseCpu(string? quantity)
    {
        if (string.IsNullOrWhiteSpace(quantity)) return null;
        var suffix = quantity.EndsWith("n", StringComparison.Ordinal) ? "n" : quantity.EndsWith("u", StringComparison.Ordinal) ? "u" : quantity.EndsWith("m", StringComparison.Ordinal) ? "m" : string.Empty;
        var number = suffix.Length == 0 ? quantity : quantity[..^suffix.Length];
        if (!double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)) return null;
        return suffix switch { "n" => value, "u" => value * 1_000, "m" => value * 1_000_000, _ => value * 1_000_000_000 };
    }

    public double? ParseMemory(string? quantity)
    {
        if (string.IsNullOrWhiteSpace(quantity)) return null;
        var suffixes = new[] { "Ki", "Mi", "Gi", "Ti", "Pi", "Ei", "K", "M", "G", "T", "P", "E" };
        var suffix = suffixes.FirstOrDefault(x => quantity.EndsWith(x, StringComparison.Ordinal)) ?? string.Empty;
        var number = suffix.Length == 0 ? quantity : quantity[..^suffix.Length];
        if (!double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)) return null;
        var exponent = suffix switch { "Ki" => 1, "Mi" => 2, "Gi" => 3, "Ti" => 4, "Pi" => 5, "Ei" => 6, "K" => 1, "M" => 2, "G" => 3, "T" => 4, "P" => 5, "E" => 6, _ => 0 };
        var baseValue = suffix is "K" or "M" or "G" or "T" or "P" or "E" ? 1000d : 1024d;
        return value * Math.Pow(baseValue, exponent);
    }
}
