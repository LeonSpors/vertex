namespace Vertex.Domain.Entities;

public sealed class Deployment
{
    private Deployment() { }

    public Deployment(Guid id, string name, string @namespace, string image, int replicas, int port, string? ingressHost)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Deployment name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(@namespace)) throw new ArgumentException("Namespace is required.", nameof(@namespace));
        if (string.IsNullOrWhiteSpace(image)) throw new ArgumentException("Container image is required.", nameof(image));
        if (replicas < 0) throw new ArgumentOutOfRangeException(nameof(replicas));
        if (port is < 1 or > 65535) throw new ArgumentOutOfRangeException(nameof(port));

        Id = id;
        Name = name;
        Namespace = @namespace;
        Image = image;
        Replicas = replicas;
        AvailableReplicas = replicas;
        Port = port;
        IngressHost = ingressHost;
        CreatedAt = DateTimeOffset.UtcNow;
        Status = DeploymentStatus.Healthy;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Namespace { get; private set; } = string.Empty;
    public string Image { get; private set; } = string.Empty;
    public DeploymentStatus Status { get; private set; }
    public int Replicas { get; private set; }
    public int AvailableReplicas { get; private set; }
    public int Port { get; private set; }
    public string? IngressHost { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public void Scale(int replicas)
    {
        if (replicas is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(replicas));
        Replicas = replicas;
        AvailableReplicas = replicas;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkRestarted()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
        Status = DeploymentStatus.Healthy;
    }
}

public enum DeploymentStatus
{
    Healthy,
    Progressing,
    Degraded,
    Unknown
}

