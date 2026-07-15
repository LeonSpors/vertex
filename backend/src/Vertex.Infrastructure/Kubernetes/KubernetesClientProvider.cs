using k8s;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vertex.Application.Abstractions;
using Vertex.Application.Contracts;

namespace Vertex.Infrastructure.Kubernetes;

/// <summary>
/// Owns Kubernetes client creation and connection diagnostics. The client is created only
/// when an operation needs it, so a missing kubeconfig never prevents the API from starting.
/// </summary>
public sealed class KubernetesClientProvider(
    IOptions<KubernetesOptions> options,
    ILogger<KubernetesClientProvider> logger) : IClusterSetupService, IDisposable
{
    private readonly KubernetesOptions settings = options.Value;
    private readonly SemaphoreSlim gate = new(1, 1);
    private IKubernetes? client;
    private string? version;
    private string? error;
    private DateTimeOffset retryAfter = DateTimeOffset.MinValue;

    public async Task<IKubernetes?> GetClientAsync(CancellationToken cancellationToken)
    {
        if (!IsClusterMode()) return null;
        if (client is not null) return client;
        if (DateTimeOffset.UtcNow < retryAfter) return null;

        await gate.WaitAsync(cancellationToken);
        try
        {
            if (client is not null) return client;
            if (DateTimeOffset.UtcNow < retryAfter) return null;

            try
            {
                var candidate = CreateClient();
                var serverVersion = await candidate.Version.GetCodeAsync(cancellationToken);
                client = candidate;
                version = serverVersion.GitVersion ?? $"v{serverVersion.Major}.{serverVersion.Minor}";
                error = null;
                logger.LogInformation("Connected to Kubernetes cluster {Version}", version);
                return client;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                error = exception switch
                {
                    FileNotFoundException => "The configured kubeconfig file was not found.",
                    UnauthorizedAccessException => "The API process cannot read the configured kubeconfig file.",
                    _ => "Kubernetes API access could not be established. Check kubeconfig, context, and service-account permissions."
                };
                logger.LogWarning(exception, "Kubernetes connection is not ready: {Reason}", error);
                retryAfter = DateTimeOffset.UtcNow.AddSeconds(5);
                return null;
            }
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<ClusterSetupResponse> GetAsync(CancellationToken cancellationToken)
    {
        if (!IsClusterMode())
        {
            return new ClusterSetupResponse("Demo", "Demo", "v1.30.2", null, Array.Empty<ClusterSetupStep>());
        }

        if (await GetClientAsync(cancellationToken) is not null)
        {
            return new ClusterSetupResponse("Connected", "Cluster", version, null, Array.Empty<ClusterSetupStep>());
        }

        return new ClusterSetupResponse("SetupRequired", "Cluster", null, error, BuildSetupSteps());
    }

    public async Task<ClusterSetupResponse> CheckAsync(CancellationToken cancellationToken)
    {
        retryAfter = DateTimeOffset.MinValue;
        return await GetAsync(cancellationToken);
    }

    public void Dispose() => gate.Dispose();

    private bool IsClusterMode() => settings.Mode.Equals("Cluster", StringComparison.OrdinalIgnoreCase);

    private IKubernetes CreateClient()
    {
        var configuration = string.IsNullOrWhiteSpace(settings.KubeConfigPath) && string.IsNullOrWhiteSpace(settings.Context)
            ? KubernetesClientConfiguration.BuildDefaultConfig()
            : KubernetesClientConfiguration.BuildConfigFromConfigFile(settings.KubeConfigPath, settings.Context, null, false);
        configuration.HttpClientTimeout = TimeSpan.FromSeconds(Math.Clamp(settings.ConnectionTimeoutSeconds, 1, 120));

        return new k8s.Kubernetes(configuration);
    }

    private IReadOnlyList<ClusterSetupStep> BuildSetupSteps() =>
    [
        new(1, "Provide cluster credentials", "Make a kubeconfig available to the API process, or run Vertex inside Kubernetes so it can use its service account.",
            "$env:KUBECONFIG = \"C:\\path\\to\\kubeconfig\""),
        new(2, "Select cluster mode", "Cluster mode is intentionally configuration-driven. Restart the API after changing its connection settings.",
            "$env:Kubernetes__Mode = \"Cluster\""),
        new(3, "Verify access", "The configured identity needs read access to cluster resources and write access to namespaces and Vertex-managed workloads.",
            "kubectl auth can-i get nodes"),
        new(4, "Enable telemetry", "Install metrics-server to populate CPU and memory cards. The rest of the platform works without it.",
            "helm repo add metrics-server https://kubernetes-sigs.github.io/metrics-server/; helm upgrade --install metrics-server metrics-server/metrics-server --namespace kube-system")
    ];
}
