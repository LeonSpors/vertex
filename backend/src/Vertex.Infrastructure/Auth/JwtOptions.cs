namespace Vertex.Infrastructure.Auth;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "vertex";
    public string Audience { get; set; } = "vertex-ui";
    public string Key { get; set; } = "vertex-development-key-change-me-please-32-chars";
    public int ExpirationMinutes { get; set; } = 480;
}

