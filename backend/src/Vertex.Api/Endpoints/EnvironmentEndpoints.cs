using FastEndpoints;
using Vertex.Application.Abstractions;
using Vertex.Application.Common;
using Vertex.Application.Contracts;

namespace Vertex.Api.Endpoints;

public sealed class ListEnvironmentsEndpoint(IEnvironmentService service) : EndpointWithoutRequest<ApiResponse<IReadOnlyList<EnvironmentSummary>>>
{
    public override void Configure() { Get("/api/environments"); AuthSchemes("Bearer"); }
    public override async Task HandleAsync(CancellationToken cancellationToken) => await SendAsync(ApiResponse<IReadOnlyList<EnvironmentSummary>>.Ok(await service.ListAsync(cancellationToken)), cancellation: cancellationToken);
}

public sealed class CreateEnvironmentEndpoint(IEnvironmentService service) : Endpoint<CreateEnvironmentRequest, ApiResponse<EnvironmentSummary>>
{
    public override void Configure() { Post("/api/environments"); AuthSchemes("Bearer"); }
    public override async Task HandleAsync(CreateEnvironmentRequest request, CancellationToken cancellationToken) => await SendAsync(ApiResponse<EnvironmentSummary>.Ok(await service.CreateAsync(request, "admin@vertex.local", cancellationToken)), StatusCodes.Status201Created, cancellationToken);
}

public sealed class DeleteEnvironmentEndpoint(IEnvironmentService service) : EndpointWithoutRequest<ApiResponse<object>>
{
    public override void Configure() { Delete("/api/environments/{id:guid}"); AuthSchemes("Bearer"); }
    public override async Task HandleAsync(CancellationToken cancellationToken) { await service.DeleteAsync(Route<Guid>("id"), cancellationToken); await SendAsync(ApiResponse<object>.Ok(new { }), cancellation: cancellationToken); }
}
