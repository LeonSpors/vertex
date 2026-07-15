using FastEndpoints;
using Vertex.Application.Abstractions;
using Vertex.Application.Common;
using Vertex.Application.Contracts;

namespace Vertex.Api.Endpoints;

public sealed class GetDashboardEndpoint(IDashboardService service) : EndpointWithoutRequest<ApiResponse<DashboardResponse>>
{
    public override void Configure() { Get("/api/dashboard"); AuthSchemes("Bearer"); }
    public override async Task HandleAsync(CancellationToken cancellationToken) => await SendAsync(ApiResponse<DashboardResponse>.Ok(await service.GetAsync(cancellationToken)), cancellation: cancellationToken);
}
