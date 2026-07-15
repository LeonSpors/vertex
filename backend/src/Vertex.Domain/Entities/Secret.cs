namespace Vertex.Domain.Entities;

public sealed class Secret
{
    private Secret() { }

    public Secret(Guid id, string name, string @namespace, IDictionary<string, string> values)
    {
        Id = id;
        Name = name;
        Namespace = @namespace;
        Values = new Dictionary<string, string>(values, StringComparer.Ordinal);
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Namespace { get; private set; } = string.Empty;
    public Dictionary<string, string> Values { get; private set; } = new(StringComparer.Ordinal);
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public void Update(IDictionary<string, string> values)
    {
        Values = new Dictionary<string, string>(values, StringComparer.Ordinal);
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

