namespace Vertex.Application.Contracts;

public sealed record LoginRequest(string Email, string Password);
public sealed record LoginResponse(string Token, string Email, string DisplayName, DateTimeOffset ExpiresAt);

