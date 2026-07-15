using FastEndpoints;
using Vertex.Application.Abstractions;
using Vertex.Application.Common;
using Vertex.Application.Contracts;

namespace Vertex.Api.Endpoints;

public sealed class LoginEndpoint(ITokenService tokenService) : Endpoint<LoginRequest, ApiResponse<LoginResponse>>
{
    public override void Configure()
    {
        Post("/api/auth/login");
        AllowAnonymous();
    }

    public override async Task HandleAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await tokenService.AuthenticateAsync(request, cancellationToken);
        if (response is null)
        {
            await SendAsync(ApiResponse<LoginResponse>.Fail("Invalid email or password."), StatusCodes.Status401Unauthorized, cancellationToken);
            return;
        }
        await SendAsync(ApiResponse<LoginResponse>.Ok(response), cancellation: cancellationToken);
    }
}

