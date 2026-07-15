using FastEndpoints;
using Vertex.Application.Abstractions;
using Vertex.Application.Common;
using Vertex.Application.Contracts;

namespace Vertex.Api.Endpoints;

public sealed class ListDatabasesEndpoint(IDatabaseService service) : EndpointWithoutRequest<ApiResponse<IReadOnlyList<DatabaseSummary>>>
{
    public override void Configure() { Get("/api/databases"); AuthSchemes("Bearer"); }
    public override async Task HandleAsync(CancellationToken cancellationToken) => await SendAsync(ApiResponse<IReadOnlyList<DatabaseSummary>>.Ok(await service.ListAsync(cancellationToken)), cancellation: cancellationToken);
}

public sealed class CreateDatabaseEndpoint(IDatabaseService service) : Endpoint<CreateDatabaseRequest, ApiResponse<DatabaseSummary>>
{
    public override void Configure() { Post("/api/databases"); AuthSchemes("Bearer"); }
    public override async Task HandleAsync(CreateDatabaseRequest request, CancellationToken cancellationToken) => await SendAsync(ApiResponse<DatabaseSummary>.Ok(await service.CreateAsync(request, cancellationToken)), StatusCodes.Status201Created, cancellationToken);
}

public sealed class DeleteDatabaseEndpoint(IDatabaseService service) : EndpointWithoutRequest<ApiResponse<object>>
{
    public override void Configure() { Delete("/api/databases/{id:guid}"); AuthSchemes("Bearer"); }
    public override async Task HandleAsync(CancellationToken cancellationToken) { await service.DeleteAsync(Route<Guid>("id"), cancellationToken); await SendAsync(ApiResponse<object>.Ok(new { }), cancellation: cancellationToken); }
}
