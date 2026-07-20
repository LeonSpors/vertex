using Vertex.Application.Contracts;

namespace Vertex.Application.Abstractions;

public interface IDashboardService
{
    Task<DashboardResponse> GetAsync(CancellationToken cancellationToken);
}

public interface IApplicationService
{
    Task<IReadOnlyList<ApplicationSummary>> ListAsync(CancellationToken cancellationToken);
    Task<ApplicationSummary> DeployAsync(DeployApplicationRequest request, CancellationToken cancellationToken);
    Task ScaleAsync(Guid id, ScaleApplicationRequest request, CancellationToken cancellationToken);
    Task RestartAsync(Guid id, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

public interface IEnvironmentService
{
    Task<IReadOnlyList<EnvironmentSummary>> ListAsync(CancellationToken cancellationToken);
    Task<EnvironmentSummary> CreateAsync(CreateEnvironmentRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

public interface ISecretService
{
    Task<IReadOnlyList<SecretSummary>> ListAsync(CancellationToken cancellationToken);
    Task<SecretDetail?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<SecretDetail> UpsertAsync(Guid? id, UpsertSecretRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

public interface IDatabaseService
{
    Task<IReadOnlyList<DatabaseSummary>> ListAsync(CancellationToken cancellationToken);
    Task<DatabaseSummary> CreateAsync(CreateDatabaseRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

public interface ILogService
{
    Task<LogsResponse> GetAsync(string application, string? pod, CancellationToken cancellationToken);
}
