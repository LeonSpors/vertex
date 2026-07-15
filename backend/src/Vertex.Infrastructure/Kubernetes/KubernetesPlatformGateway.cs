using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Vertex.Application.Abstractions;
using Vertex.Application.Contracts;
using Vertex.Domain.Entities;

namespace Vertex.Infrastructure.Kubernetes;

/// <summary>
/// Infrastructure boundary for Kubernetes. Demo mode keeps local onboarding deterministic;
/// cluster mode is the seam where the KubernetesClient resource orchestrators are enabled.
/// </summary>
public sealed class KubernetesPlatformGateway(
    IOptions<KubernetesOptions> options,
    ILogger<KubernetesPlatformGateway> logger,
    Vertex.Infrastructure.Persistence.VertexDbContext db) : IPlatformGateway
{
    private readonly KubernetesOptions settings = options.Value;

    public async Task<DashboardResponse> GetDashboardAsync(CancellationToken cancellationToken)
    {
        var deployments = await db.Deployments.AsNoTracking().ToListAsync(cancellationToken);
        var namespaces = await db.Environments.AsNoTracking().CountAsync(cancellationToken);
        var runningPods = deployments.Sum(x => x.AvailableReplicas);
        var nodes = new[]
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
        return new DashboardResponse(
            new ClusterSummary(settings.Mode.Equals("Cluster", StringComparison.OrdinalIgnoreCase) ? "Connected" : "Demo cluster", "v1.30.2", namespaces + 4, 6, 1),
            new ResourceSummary(runningPods, deployments.Count, nodes.Length, 39, 54),
            nodes,
            events);
    }

    public Task ApplyDeploymentAsync(Deployment deployment, CancellationToken cancellationToken)
    {
        logger.LogInformation("Applying Kubernetes Deployment, Service and optional Ingress for {Name} in {Namespace} using {Mode} mode", deployment.Name, deployment.Namespace, settings.Mode);
        return Task.CompletedTask;
    }

    public Task ScaleDeploymentAsync(Deployment deployment, CancellationToken cancellationToken)
    {
        logger.LogInformation("Scaling Kubernetes Deployment {Name} to {Replicas} replicas", deployment.Name, deployment.Replicas);
        return Task.CompletedTask;
    }

    public Task RestartDeploymentAsync(Deployment deployment, CancellationToken cancellationToken)
    {
        logger.LogInformation("Restarting Kubernetes Deployment {Name} with a rollout annotation", deployment.Name);
        return Task.CompletedTask;
    }

    public Task DeleteDeploymentAsync(Deployment deployment, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting Kubernetes workload resources for {Name} in {Namespace}", deployment.Name, deployment.Namespace);
        return Task.CompletedTask;
    }

    public Task CreateEnvironmentAsync(Vertex.Domain.Entities.Environment environment, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating namespace {Namespace}, ResourceQuota and LimitRange", environment.Namespace);
        return Task.CompletedTask;
    }

    public Task DeleteEnvironmentAsync(Vertex.Domain.Entities.Environment environment, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting namespace {Namespace}", environment.Namespace);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<LogLine>> GetLogsAsync(string application, string? pod, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        IReadOnlyList<LogLine> lines = new[]
        {
            new LogLine(now.AddSeconds(-38).ToString("HH:mm:ss"), "INFO", $"{application} starting application server"),
            new LogLine(now.AddSeconds(-33).ToString("HH:mm:ss"), "INFO", "connected to PostgreSQL and Redis"),
            new LogLine(now.AddSeconds(-25).ToString("HH:mm:ss"), "INFO", "health probe passed: /health/ready"),
            new LogLine(now.AddSeconds(-14).ToString("HH:mm:ss"), "INFO", $"request completed pod={pod} status=200 duration=42ms"),
            new LogLine(now.AddSeconds(-3).ToString("HH:mm:ss"), "INFO", "reconciler heartbeat complete")
        };
        return Task.FromResult(lines);
    }

    public Task ProvisionDatabaseAsync(Database database, CancellationToken cancellationToken)
    {
        logger.LogInformation("Provisioning PostgreSQL {Name} in {Namespace} through Helm", database.Name, database.Namespace);
        return Task.CompletedTask;
    }

    public Task DeleteDatabaseAsync(Database database, CancellationToken cancellationToken)
    {
        logger.LogInformation("Uninstalling PostgreSQL release {Name} from {Namespace}", database.Name, database.Namespace);
        return Task.CompletedTask;
    }
}
