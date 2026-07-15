using FastEndpoints;
using Vertex.Application.Abstractions;
using Vertex.Application.Common;
using Vertex.Application.Contracts;

namespace Vertex.Api.Endpoints;

public sealed class GetClusterSetupEndpoint(IClusterSetupService service) : EndpointWithoutRequest<ApiResponse<ClusterSetupResponse>>
{
    public override void Configure() { Get("/api/setup/cluster"); AuthSchemes("Bearer"); }

    public override async Task HandleAsync(CancellationToken cancellationToken)
        => await SendAsync(ApiResponse<ClusterSetupResponse>.Ok(await service.GetAsync(cancellationToken)), cancellation: cancellationToken);
}

public sealed class CheckClusterSetupEndpoint(IClusterSetupService service) : EndpointWithoutRequest<ApiResponse<ClusterSetupResponse>>
{
    public override void Configure() { Post("/api/setup/cluster/check"); AuthSchemes("Bearer"); }

    public override async Task HandleAsync(CancellationToken cancellationToken)
        => await SendAsync(ApiResponse<ClusterSetupResponse>.Ok(await service.CheckAsync(cancellationToken)), cancellation: cancellationToken);
}
