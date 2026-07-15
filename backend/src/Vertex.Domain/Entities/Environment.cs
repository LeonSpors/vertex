namespace Vertex.Domain.Entities;

public sealed class Environment
{
    private Environment() { }

    public Environment(Guid id, string name, string @namespace, string owner)
    {
        Id = id;
        Name = name;
        Namespace = @namespace;
        Owner = owner;
        Status = EnvironmentStatus.Ready;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Namespace { get; private set; } = string.Empty;
    public string Owner { get; private set; } = string.Empty;
    public EnvironmentStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
}

public enum EnvironmentStatus { Ready, Provisioning, Deleting, Failed }

