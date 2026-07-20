namespace Vertex.Application.Contracts;

public sealed record SecretSummary(Guid Id, string Name, string Namespace, int KeyCount, DateTimeOffset UpdatedAt);
public sealed record SecretDetail(Guid Id, string Name, string Namespace, IReadOnlyList<string> Keys, DateTimeOffset UpdatedAt);
public sealed record UpsertSecretRequest(string Name, string Namespace, Dictionary<string, string> Values);
