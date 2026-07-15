namespace Vertex.Infrastructure.Kubernetes;

public sealed class KubernetesOptions
{
    public string Mode { get; set; } = "Demo";
    public string DefaultNamespace { get; set; } = "vertex";
    public string? KubeConfigPath { get; set; }
    public string? Context { get; set; }
    public int ConnectionTimeoutSeconds { get; set; } = 15;
}
