namespace Vertex.Application.Contracts;

public sealed record DatabaseSummary(Guid Id, string Name, string Namespace, string Host, int Port, string Username, string CredentialSecretName, string Status, DateTimeOffset CreatedAt);
public sealed record CreateDatabaseRequest(string Name, string Namespace);
