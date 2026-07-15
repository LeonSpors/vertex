using Vertex.Domain.Entities;

namespace Vertex.Application.Contracts;

public sealed record ApplicationSummary(
    Guid Id,
    string Name,
    string Namespace,
    string Image,
    string Status,
    int Replicas,
    int AvailableReplicas,
    int Port,
    string? IngressHost,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record DeployApplicationRequest(string Name, string Namespace, string Image, int Replicas, int Port, string? IngressHost);
public sealed record ScaleApplicationRequest(int Replicas);

