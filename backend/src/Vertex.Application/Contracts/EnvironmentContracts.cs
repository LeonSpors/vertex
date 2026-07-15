namespace Vertex.Application.Contracts;

public sealed record EnvironmentSummary(Guid Id, string Name, string Namespace, string Owner, string Status, DateTimeOffset CreatedAt);
public sealed record CreateEnvironmentRequest(string Name, string Namespace);

