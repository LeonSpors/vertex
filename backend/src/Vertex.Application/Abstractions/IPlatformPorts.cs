using Vertex.Application.Contracts;
using Vertex.Domain.Entities;

namespace Vertex.Application.Abstractions;

public interface IPlatformGateway
{
    Task<DashboardResponse> GetDashboardAsync(CancellationToken cancellationToken);
    Task ApplyDeploymentAsync(Deployment deployment, CancellationToken cancellationToken);
    Task ScaleDeploymentAsync(Deployment deployment, CancellationToken cancellationToken);
    Task RestartDeploymentAsync(Deployment deployment, CancellationToken cancellationToken);
    Task DeleteDeploymentAsync(Deployment deployment, CancellationToken cancellationToken);
    Task CreateEnvironmentAsync(Vertex.Domain.Entities.Environment environment, CancellationToken cancellationToken);
    Task DeleteEnvironmentAsync(Vertex.Domain.Entities.Environment environment, CancellationToken cancellationToken);
    Task<IReadOnlyList<LogLine>> GetLogsAsync(string application, string @namespace, string? pod, CancellationToken cancellationToken);
    Task ProvisionDatabaseAsync(Database database, CancellationToken cancellationToken);
    Task DeleteDatabaseAsync(Database database, CancellationToken cancellationToken);
}

public interface IClusterSetupService
{
    Task<ClusterSetupResponse> GetAsync(CancellationToken cancellationToken);
    Task<ClusterSetupResponse> CheckAsync(CancellationToken cancellationToken);
}

public interface ITokenService
{
    Task<LoginResponse?> AuthenticateAsync(LoginRequest request, CancellationToken cancellationToken);
}
