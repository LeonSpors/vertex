using FastEndpoints;
using Vertex.Application.Abstractions;
using Vertex.Application.Common;
using Vertex.Application.Contracts;

namespace Vertex.Api.Endpoints;

public sealed class ListApplicationsEndpoint(IApplicationService service) : EndpointWithoutRequest<ApiResponse<IReadOnlyList<ApplicationSummary>>>
{
    public override void Configure() { Get("/api/applications"); AuthSchemes("Bearer"); }
    public override async Task HandleAsync(CancellationToken cancellationToken) => await SendAsync(ApiResponse<IReadOnlyList<ApplicationSummary>>.Ok(await service.ListAsync(cancellationToken)), cancellation: cancellationToken);
}

public sealed class DeployApplicationEndpoint(IApplicationService service) : Endpoint<DeployApplicationRequest, ApiResponse<ApplicationSummary>>
{
    public override void Configure() { Post("/api/applications"); AuthSchemes("Bearer"); }
    public override async Task HandleAsync(DeployApplicationRequest request, CancellationToken cancellationToken) => await SendAsync(ApiResponse<ApplicationSummary>.Ok(await service.DeployAsync(request, cancellationToken)), StatusCodes.Status201Created, cancellationToken);
}

public sealed class ScaleApplicationEndpoint(IApplicationService service) : Endpoint<ScaleApplicationRequest, ApiResponse<object>>
{
    public override void Configure() { Put("/api/applications/{id:guid}/scale"); AuthSchemes("Bearer"); }
    public override async Task HandleAsync(ScaleApplicationRequest request, CancellationToken cancellationToken) { await service.ScaleAsync(Route<Guid>("id"), request, cancellationToken); await SendAsync(ApiResponse<object>.Ok(new { }), cancellation: cancellationToken); }
}

public sealed class RestartApplicationEndpoint(IApplicationService service) : EndpointWithoutRequest<ApiResponse<object>>
{
    public override void Configure() { Post("/api/applications/{id:guid}/restart"); AuthSchemes("Bearer"); }
    public override async Task HandleAsync(CancellationToken cancellationToken) { await service.RestartAsync(Route<Guid>("id"), cancellationToken); await SendAsync(ApiResponse<object>.Ok(new { }), cancellation: cancellationToken); }
}

public sealed class DeleteApplicationEndpoint(IApplicationService service) : EndpointWithoutRequest<ApiResponse<object>>
{
    public override void Configure() { Delete("/api/applications/{id:guid}"); AuthSchemes("Bearer"); }
    public override async Task HandleAsync(CancellationToken cancellationToken) { await service.DeleteAsync(Route<Guid>("id"), cancellationToken); await SendAsync(ApiResponse<object>.Ok(new { }), cancellation: cancellationToken); }
}
