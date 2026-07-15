using FastEndpoints;
using Vertex.Application.Abstractions;
using Vertex.Application.Common;
using Vertex.Application.Contracts;

namespace Vertex.Api.Endpoints;

public sealed class LogsRequest
{
    public string Application { get; set; } = string.Empty;
    public string? Pod { get; set; }
}

public sealed class GetLogsEndpoint(ILogService service) : Endpoint<LogsRequest, ApiResponse<LogsResponse>>
{
    public override void Configure() { Get("/api/logs"); AuthSchemes("Bearer"); }
    public override async Task HandleAsync(LogsRequest request, CancellationToken cancellationToken) => await SendAsync(ApiResponse<LogsResponse>.Ok(await service.GetAsync(request.Application, request.Pod, cancellationToken)), cancellation: cancellationToken);
}
