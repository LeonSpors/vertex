using System.Security.Claims;
using Vertex.Application.Abstractions;

namespace Vertex.Api.Security;

public sealed class HttpCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal Principal => httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal();

    public string? Email => Principal.FindFirstValue(ClaimTypes.Email)
        ?? Principal.FindFirstValue("email")
        ?? Principal.Identity?.Name;

    public bool CanAccessNamespace(string @namespace) => Principal.IsInRole("platform-admin")
        || Principal.FindAll("namespace").Any(claim => string.Equals(claim.Value, @namespace, StringComparison.OrdinalIgnoreCase));

    public void DemandNamespace(string @namespace)
    {
        if (!CanAccessNamespace(@namespace))
        {
            throw new UnauthorizedAccessException("You do not have access to the requested namespace.");
        }
    }
}
