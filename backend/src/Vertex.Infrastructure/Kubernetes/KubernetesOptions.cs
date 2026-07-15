namespace Vertex.Infrastructure.Kubernetes;

public sealed class KubernetesOptions
{
    public string Mode { get; set; } = "Demo";
    public string DefaultNamespace { get; set; } = "vertex";
}

