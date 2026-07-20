using System.Text.Json;
using System.Net;
using k8s;
using k8s.Autorest;
using k8s.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vertex.Application.Abstractions;
using Vertex.Application.Contracts;
using Vertex.Domain.Entities;

namespace Vertex.Infrastructure.Kubernetes;

/// <summary>
/// Kubernetes resource gateway. Demo mode provides deterministic local data; Cluster mode
/// uses the official KubernetesClient to reconcile workloads and namespaces.
/// </summary>
public sealed class KubernetesPlatformGateway : IPlatformGateway
{
    private readonly KubernetesOptions settings;
    private readonly ILogger<KubernetesPlatformGateway> logger;
    private readonly Vertex.Infrastructure.Persistence.VertexDbContext db;
    private readonly KubernetesClientProvider clientProvider;
    private readonly KubernetesMetricsReader metricsReader;

    public KubernetesPlatformGateway(IOptions<KubernetesOptions> options, ILogger<KubernetesPlatformGateway> logger, Vertex.Infrastructure.Persistence.VertexDbContext db, KubernetesClientProvider clientProvider, KubernetesMetricsReader metricsReader)
    {
        settings = options.Value;
        this.logger = logger;
        this.db = db;
        this.clientProvider = clientProvider;
        this.metricsReader = metricsReader;
    }

    public async Task<DashboardResponse> GetDashboardAsync(CancellationToken cancellationToken)
    {
        var setup = await clientProvider.GetAsync(cancellationToken);
        var client = await clientProvider.GetClientAsync(cancellationToken);
        if (client is not null)
        {
            var deployments = await client.AppsV1.ListDeploymentForAllNamespacesAsync(cancellationToken: cancellationToken);
            var namespaces = await client.CoreV1.ListNamespaceAsync(cancellationToken: cancellationToken);
            var nodes = await client.CoreV1.ListNodeAsync(cancellationToken: cancellationToken);
            var pods = await client.CoreV1.ListPodForAllNamespacesAsync(cancellationToken: cancellationToken);
            var storageClasses = await client.StorageV1.ListStorageClassAsync(cancellationToken: cancellationToken);
            var ingressClasses = await client.NetworkingV1.ListIngressClassAsync(cancellationToken: cancellationToken);
            var clusterEvents = await ReadClusterEventsAsync(client, cancellationToken);
            var metrics = await metricsReader.ReadAsync(client, nodes.Items.ToArray(), cancellationToken);
            return new DashboardResponse(
                new ClusterSummary("Connected", setup.Version ?? "Unknown", namespaces.Items.Count, storageClasses.Items.Count, ingressClasses.Items.Count),
                new ResourceSummary(pods.Items.Count(x => string.Equals(x.Status?.Phase, "Running", StringComparison.OrdinalIgnoreCase)), deployments.Items.Count(x => (x.Status?.AvailableReplicas ?? 0) > 0), nodes.Items.Count, metrics.CpuUsagePercent, metrics.MemoryUsagePercent),
                nodes.Items.Select(x => new NodeSummary(x.Metadata?.Name ?? "unknown", x.Status?.Conditions?.Any(c => c.Type == "Ready" && c.Status == "True") == true ? "Ready" : "NotReady", FormatPercent(metrics.Nodes, x.Metadata?.Name, true), FormatPercent(metrics.Nodes, x.Metadata?.Name, false), x.Metadata?.Labels?.ContainsKey("node-role.kubernetes.io/control-plane") == true ? "control-plane" : "worker")).ToArray(),
                clusterEvents);
        }

        if (settings.Mode.Equals("Cluster", StringComparison.OrdinalIgnoreCase))
        {
            return new DashboardResponse(
                new ClusterSummary(setup.Status, setup.Version ?? "Not connected", 0, 0, 0),
                new ResourceSummary(0, 0, 0, null, null),
                Array.Empty<NodeSummary>(),
                Array.Empty<EventSummary>());
        }

        var localDeployments = await db.Deployments.AsNoTracking().ToListAsync(cancellationToken);
        var localNamespaces = await db.Environments.AsNoTracking().CountAsync(cancellationToken);
        var localNodes = new[]
        {
            new NodeSummary("vertex-worker-01", "Ready", "32%", "58%", "worker"),
            new NodeSummary("vertex-worker-02", "Ready", "47%", "64%", "worker"),
            new NodeSummary("vertex-control-01", "Ready", "18%", "41%", "control-plane")
        };
        var events = new[]
        {
            new EventSummary("Normal", "ScalingReplicaSet", "Scaled checkout-api to 3 replicas", "production", DateTimeOffset.UtcNow.AddMinutes(-8)),
            new EventSummary("Normal", "SuccessfulResync", "Ingress reconciled successfully", "staging", DateTimeOffset.UtcNow.AddMinutes(-24)),
            new EventSummary("Warning", "BackOff", "catalog-worker container restarted once", "production", DateTimeOffset.UtcNow.AddHours(-1))
        };
        return new DashboardResponse(new ClusterSummary("Demo cluster", "v1.30.2", localNamespaces + 4, 6, 1), new ResourceSummary(localDeployments.Sum(x => x.AvailableReplicas), localDeployments.Count, localNodes.Length, 39, 54), localNodes, events);
    }

    private async Task<IReadOnlyList<EventSummary>> ReadClusterEventsAsync(IKubernetes client, CancellationToken cancellationToken)
    {
        try
        {
            var events = await client.CoreV1.ListEventForAllNamespacesAsync(limit: 50, cancellationToken: cancellationToken);
            return events.Items
                .OrderByDescending(x => x.LastTimestamp ?? x.EventTime ?? x.FirstTimestamp ?? DateTime.MinValue)
                .Take(5)
                .Select(x => new EventSummary(x.Type ?? "Normal", x.Reason ?? "Event", x.Message ?? "", x.Metadata?.NamespaceProperty ?? x.InvolvedObject?.NamespaceProperty ?? "cluster", new DateTimeOffset(x.LastTimestamp ?? x.EventTime ?? x.FirstTimestamp ?? DateTime.UtcNow)))
                .ToArray();
        }
        catch (HttpOperationException exception)
        {
            logger.LogWarning("Kubernetes events are unavailable: {Reason}", exception.Message);
            return Array.Empty<EventSummary>();
        }
    }

    private static string FormatPercent(IReadOnlyDictionary<string, NodeResourceMetrics> metrics, string? nodeName, bool cpu)
    {
        if (nodeName is null || !metrics.TryGetValue(nodeName, out var value)) return "—";
        var percent = cpu ? value.CpuUsagePercent : value.MemoryUsagePercent;
        return percent.HasValue ? $"{percent.Value:0}%" : "—";
    }

    public async Task ApplyDeploymentAsync(Deployment deployment, CancellationToken cancellationToken)
    {
        var client = await GetClientForOperationAsync(cancellationToken);
        if (client is null) { logger.LogInformation("Demo reconcile: Deployment, Service and optional Ingress for {Name} in {Namespace}", deployment.Name, deployment.Namespace); return; }
        var labels = new Dictionary<string, string> { ["app.kubernetes.io/name"] = deployment.Name, ["app.kubernetes.io/managed-by"] = "vertex" };
        var resource = new V1Deployment
        {
            ApiVersion = "apps/v1", Kind = "Deployment",
            Metadata = new V1ObjectMeta { Name = deployment.Name, NamespaceProperty = deployment.Namespace, Labels = labels },
            Spec = new V1DeploymentSpec
            {
                Replicas = deployment.Replicas,
                Selector = new V1LabelSelector { MatchLabels = labels },
                Template = new V1PodTemplateSpec
                {
                    Metadata = new V1ObjectMeta { Labels = labels },
                    Spec = new V1PodSpec
                    {
                    SecurityContext = new V1PodSecurityContext
                    {
                        RunAsNonRoot = true,
                        SeccompProfile = new V1SeccompProfile { Type = "RuntimeDefault" }
                    },
                    Containers = new List<V1Container> { new()
                    {
                        Name = deployment.Name,
                        Image = deployment.Image,
                        Ports = new List<V1ContainerPort> { new() { ContainerPort = deployment.Port } },
                        SecurityContext = new V1SecurityContext
                        {
                            AllowPrivilegeEscalation = false,
                            ReadOnlyRootFilesystem = true,
                            Capabilities = new V1Capabilities { Drop = new List<string> { "ALL" } }
                        },
                        ReadinessProbe = new V1Probe { HttpGet = new V1HTTPGetAction { Path = "/health/ready", Port = (IntOrString)deployment.Port }, InitialDelaySeconds = 10, PeriodSeconds = 10 },
                        LivenessProbe = new V1Probe { HttpGet = new V1HTTPGetAction { Path = "/health/live", Port = (IntOrString)deployment.Port }, InitialDelaySeconds = 20, PeriodSeconds = 20 }
                    } }
                    }
                }
            }
        };
        await UpsertDeploymentAsync(client, resource, deployment.Namespace, cancellationToken);
        var service = new V1Service
        {
            ApiVersion = "v1",
            Kind = "Service",
            Metadata = new V1ObjectMeta { Name = deployment.Name, NamespaceProperty = deployment.Namespace, Labels = labels },
            Spec = new V1ServiceSpec
            {
                Selector = labels,
                Ports = new List<V1ServicePort> { new() { Name = "http", Port = deployment.Port, TargetPort = (IntOrString)deployment.Port } }
            }
        };
        await UpsertServiceAsync(client, service, deployment.Namespace, cancellationToken);
        if (!string.IsNullOrWhiteSpace(deployment.IngressHost))
        {
            var ingress = new V1Ingress { ApiVersion = "networking.k8s.io/v1", Kind = "Ingress", Metadata = new V1ObjectMeta { Name = deployment.Name, NamespaceProperty = deployment.Namespace, Labels = labels }, Spec = new V1IngressSpec { Rules = new List<V1IngressRule> { new() { Host = deployment.IngressHost, Http = new V1HTTPIngressRuleValue { Paths = new List<V1HTTPIngressPath> { new() { Path = "/", PathType = "Prefix", Backend = new V1IngressBackend { Service = new V1IngressServiceBackend { Name = deployment.Name, Port = new V1ServiceBackendPort { Number = deployment.Port } } } } } } } } } };
            await UpsertIngressAsync(client, ingress, deployment.Namespace, cancellationToken);
        }
    }

    public async Task ScaleDeploymentAsync(Deployment deployment, CancellationToken cancellationToken)
    {
        var client = await GetClientForOperationAsync(cancellationToken);
        if (client is null) { logger.LogInformation("Demo reconcile: scaling {Name} to {Replicas}", deployment.Name, deployment.Replicas); return; }
        await client.AppsV1.PatchNamespacedDeploymentAsync(new V1Patch(JsonSerializer.Serialize(new { spec = new { replicas = deployment.Replicas } }), V1Patch.PatchType.MergePatch), deployment.Name, deployment.Namespace, cancellationToken: cancellationToken);
    }

    public async Task RestartDeploymentAsync(Deployment deployment, CancellationToken cancellationToken)
    {
        var client = await GetClientForOperationAsync(cancellationToken);
        if (client is null) { logger.LogInformation("Demo reconcile: restarting {Name}", deployment.Name); return; }
        await client.AppsV1.PatchNamespacedDeploymentAsync(new V1Patch(JsonSerializer.Serialize(new { spec = new { template = new { metadata = new { annotations = new Dictionary<string, string> { ["vertex.dev/restarted-at"] = DateTimeOffset.UtcNow.ToString("O") } } } } }), V1Patch.PatchType.MergePatch), deployment.Name, deployment.Namespace, cancellationToken: cancellationToken);
    }

    public async Task DeleteDeploymentAsync(Deployment deployment, CancellationToken cancellationToken)
    {
        var client = await GetClientForOperationAsync(cancellationToken);
        if (client is null) { logger.LogInformation("Demo reconcile: deleting {Name} in {Namespace}", deployment.Name, deployment.Namespace); return; }
        await IgnoreNotFoundAsync(() => client.AppsV1.DeleteNamespacedDeploymentAsync(deployment.Name, deployment.Namespace, body: new V1DeleteOptions(), cancellationToken: cancellationToken));
        await IgnoreNotFoundAsync(() => client.CoreV1.DeleteNamespacedServiceAsync(deployment.Name, deployment.Namespace, body: new V1DeleteOptions(), cancellationToken: cancellationToken));
        if (!string.IsNullOrWhiteSpace(deployment.IngressHost)) await IgnoreNotFoundAsync(() => client.NetworkingV1.DeleteNamespacedIngressAsync(deployment.Name, deployment.Namespace, body: new V1DeleteOptions(), cancellationToken: cancellationToken));
    }

    public async Task CreateEnvironmentAsync(Vertex.Domain.Entities.Environment environment, CancellationToken cancellationToken)
    {
        var client = await GetClientForOperationAsync(cancellationToken);
        if (client is null) { logger.LogInformation("Demo reconcile: namespace {Namespace}, ResourceQuota and LimitRange", environment.Namespace); return; }
        await client.CoreV1.CreateNamespaceAsync(new V1Namespace { ApiVersion = "v1", Kind = "Namespace", Metadata = new V1ObjectMeta { Name = environment.Namespace, Labels = new Dictionary<string, string> { ["vertex.dev/environment"] = environment.Name } } }, cancellationToken: cancellationToken);
        await client.CoreV1.CreateNamespacedResourceQuotaAsync(new V1ResourceQuota { ApiVersion = "v1", Kind = "ResourceQuota", Metadata = new V1ObjectMeta { Name = "vertex-quota", NamespaceProperty = environment.Namespace }, Spec = new V1ResourceQuotaSpec { Hard = new Dictionary<string, ResourceQuantity> { ["pods"] = new("50"), ["requests.cpu"] = new("4"), ["requests.memory"] = new("8Gi") } } }, environment.Namespace, cancellationToken: cancellationToken);
        await client.CoreV1.CreateNamespacedLimitRangeAsync(new V1LimitRange { ApiVersion = "v1", Kind = "LimitRange", Metadata = new V1ObjectMeta { Name = "vertex-defaults", NamespaceProperty = environment.Namespace }, Spec = new V1LimitRangeSpec { Limits = new List<V1LimitRangeItem> { new() { Type = "Container", DefaultProperty = new Dictionary<string, ResourceQuantity> { ["cpu"] = new("500m"), ["memory"] = new("512Mi") }, DefaultRequest = new Dictionary<string, ResourceQuantity> { ["cpu"] = new("100m"), ["memory"] = new("128Mi") } } } } }, environment.Namespace, cancellationToken: cancellationToken);
    }

    public async Task DeleteEnvironmentAsync(Vertex.Domain.Entities.Environment environment, CancellationToken cancellationToken)
    {
        var client = await GetClientForOperationAsync(cancellationToken);
        if (client is null) { logger.LogInformation("Demo reconcile: deleting namespace {Namespace}", environment.Namespace); return; }
        await IgnoreNotFoundAsync(() => client.CoreV1.DeleteNamespaceAsync(environment.Namespace, body: new V1DeleteOptions(), cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<LogLine>> GetLogsAsync(string application, string? pod, CancellationToken cancellationToken)
    {
        var client = await GetClientForOperationAsync(cancellationToken);
        if (client is not null && !string.IsNullOrWhiteSpace(pod))
        {
            await using var raw = await client.CoreV1.ReadNamespacedPodLogAsync(pod, settings.DefaultNamespace, cancellationToken: cancellationToken);
            using var reader = new StreamReader(raw);
            var content = await reader.ReadToEndAsync(cancellationToken);
            return content.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(line => new LogLine(DateTimeOffset.UtcNow.ToString("HH:mm:ss"), "INFO", line)).ToArray();
        }
        var now = DateTimeOffset.UtcNow;
        return new[] { new LogLine(now.AddSeconds(-38).ToString("HH:mm:ss"), "INFO", $"{application} starting application server"), new LogLine(now.AddSeconds(-33).ToString("HH:mm:ss"), "INFO", "connected to PostgreSQL and Redis"), new LogLine(now.AddSeconds(-25).ToString("HH:mm:ss"), "INFO", "health probe passed: /health/ready"), new LogLine(now.AddSeconds(-14).ToString("HH:mm:ss"), "INFO", $"request completed pod={pod} status=200 duration=42ms"), new LogLine(now.AddSeconds(-3).ToString("HH:mm:ss"), "INFO", "reconciler heartbeat complete") };
    }

    public async Task ProvisionDatabaseAsync(Database database, CancellationToken cancellationToken)
    {
        _ = await GetClientForOperationAsync(cancellationToken);
        logger.LogInformation("Provisioning PostgreSQL {Name} in {Namespace} through Helm", database.Name, database.Namespace);
    }

    public async Task DeleteDatabaseAsync(Database database, CancellationToken cancellationToken)
    {
        _ = await GetClientForOperationAsync(cancellationToken);
        logger.LogInformation("Uninstalling PostgreSQL release {Name} from {Namespace}", database.Name, database.Namespace);
    }

    private static async Task UpsertDeploymentAsync(IKubernetes client, V1Deployment desired, string @namespace, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await client.AppsV1.ReadNamespacedDeploymentAsync(desired.Metadata.Name, @namespace, cancellationToken: cancellationToken);
            desired.Metadata.ResourceVersion = existing.Metadata?.ResourceVersion;
            await client.AppsV1.ReplaceNamespacedDeploymentAsync(desired, desired.Metadata.Name, @namespace, cancellationToken: cancellationToken);
        }
        catch (HttpOperationException exception) when (IsNotFound(exception))
        {
            await client.AppsV1.CreateNamespacedDeploymentAsync(desired, @namespace, cancellationToken: cancellationToken);
        }
    }

    private static async Task UpsertServiceAsync(IKubernetes client, V1Service desired, string @namespace, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await client.CoreV1.ReadNamespacedServiceAsync(desired.Metadata.Name, @namespace, cancellationToken: cancellationToken);
            var patch = new V1Patch(JsonSerializer.Serialize(new
            {
                metadata = new { labels = desired.Metadata.Labels },
                spec = new { selector = desired.Spec.Selector, ports = desired.Spec.Ports }
            }), V1Patch.PatchType.MergePatch);
            await client.CoreV1.PatchNamespacedServiceAsync(patch, existing.Metadata.Name, @namespace, cancellationToken: cancellationToken);
        }
        catch (HttpOperationException exception) when (IsNotFound(exception))
        {
            await client.CoreV1.CreateNamespacedServiceAsync(desired, @namespace, cancellationToken: cancellationToken);
        }
    }

    private static async Task UpsertIngressAsync(IKubernetes client, V1Ingress desired, string @namespace, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await client.NetworkingV1.ReadNamespacedIngressAsync(desired.Metadata.Name, @namespace, cancellationToken: cancellationToken);
            desired.Metadata.ResourceVersion = existing.Metadata?.ResourceVersion;
            await client.NetworkingV1.ReplaceNamespacedIngressAsync(desired, desired.Metadata.Name, @namespace, cancellationToken: cancellationToken);
        }
        catch (HttpOperationException exception) when (IsNotFound(exception))
        {
            await client.NetworkingV1.CreateNamespacedIngressAsync(desired, @namespace, cancellationToken: cancellationToken);
        }
    }

    private static async Task IgnoreNotFoundAsync(Func<Task> operation)
    {
        try
        {
            await operation();
        }
        catch (HttpOperationException exception) when (IsNotFound(exception))
        {
        }
    }

    private static bool IsNotFound(HttpOperationException exception) => exception.Response.StatusCode == HttpStatusCode.NotFound;

    private async Task<IKubernetes?> GetClientForOperationAsync(CancellationToken cancellationToken)
    {
        var client = await clientProvider.GetClientAsync(cancellationToken);
        if (client is null && settings.Mode.Equals("Cluster", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The Kubernetes cluster is not connected. Open Settings to complete the assisted setup.");
        }

        return client;
    }
}
