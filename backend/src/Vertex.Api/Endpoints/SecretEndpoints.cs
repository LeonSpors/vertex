using FastEndpoints;
using Vertex.Application.Abstractions;
using Vertex.Application.Common;
using Vertex.Application.Contracts;

namespace Vertex.Api.Endpoints;

public sealed class ListSecretsEndpoint(ISecretService service) : EndpointWithoutRequest<ApiResponse<IReadOnlyList<SecretSummary>>>
{
    public override void Configure() { Get("/api/secrets"); AuthSchemes("Bearer"); }
    public override async Task HandleAsync(CancellationToken cancellationToken) => await SendAsync(ApiResponse<IReadOnlyList<SecretSummary>>.Ok(await service.ListAsync(cancellationToken)), cancellation: cancellationToken);
}

public sealed class GetSecretEndpoint(ISecretService service) : EndpointWithoutRequest<ApiResponse<SecretDetail>>
{
    public override void Configure() { Get("/api/secrets/{id:guid}"); AuthSchemes("Bearer"); }
    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var result = await service.GetAsync(Route<Guid>("id"), cancellationToken);
        if (result is null) { await SendAsync(ApiResponse<SecretDetail>.Fail("Secret was not found."), StatusCodes.Status404NotFound, cancellationToken); return; }
        await SendAsync(ApiResponse<SecretDetail>.Ok(result), cancellation: cancellationToken);
    }
}

public sealed class CreateSecretEndpoint(ISecretService service) : Endpoint<UpsertSecretRequest, ApiResponse<SecretDetail>>
{
    public override void Configure() { Post("/api/secrets"); AuthSchemes("Bearer"); }
    public override async Task HandleAsync(UpsertSecretRequest request, CancellationToken cancellationToken) => await SendAsync(ApiResponse<SecretDetail>.Ok(await service.UpsertAsync(null, request, cancellationToken)), StatusCodes.Status201Created, cancellationToken);
}

public sealed class UpdateSecretEndpoint(ISecretService service) : Endpoint<UpsertSecretRequest, ApiResponse<SecretDetail>>
{
    public override void Configure() { Put("/api/secrets/{id:guid}"); AuthSchemes("Bearer"); }
    public override async Task HandleAsync(UpsertSecretRequest request, CancellationToken cancellationToken) => await SendAsync(ApiResponse<SecretDetail>.Ok(await service.UpsertAsync(Route<Guid>("id"), request, cancellationToken)), cancellation: cancellationToken);
}

public sealed class DeleteSecretEndpoint(ISecretService service) : EndpointWithoutRequest<ApiResponse<object>>
{
    public override void Configure() { Delete("/api/secrets/{id:guid}"); AuthSchemes("Bearer"); }
    public override async Task HandleAsync(CancellationToken cancellationToken) { await service.DeleteAsync(Route<Guid>("id"), cancellationToken); await SendAsync(ApiResponse<object>.Ok(new { }), cancellation: cancellationToken); }
}
