using Vertex.Application.Abstractions;
using Vertex.Application.Contracts;
using Vertex.Domain.Entities;

namespace Vertex.Application.Services;

public sealed class DashboardService(IPlatformGateway gateway) : IDashboardService
{
    public Task<DashboardResponse> GetAsync(CancellationToken cancellationToken) => gateway.GetDashboardAsync(cancellationToken);
}

public sealed class ApplicationService(
    IDeploymentRepository repository,
    IUnitOfWork unitOfWork,
    IPlatformGateway gateway) : IApplicationService
{
    public async Task<IReadOnlyList<ApplicationSummary>> ListAsync(CancellationToken cancellationToken)
        => (await repository.ListAsync(cancellationToken)).Select(Map).ToArray();

    public async Task<ApplicationSummary> DeployAsync(DeployApplicationRequest request, CancellationToken cancellationToken)
    {
        var deployment = new Deployment(Guid.NewGuid(), request.Name, request.Namespace, request.Image, request.Replicas, request.Port, request.IngressHost);
        await repository.AddAsync(deployment, cancellationToken);
        await gateway.ApplyDeploymentAsync(deployment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(deployment);
    }

    public async Task ScaleAsync(Guid id, ScaleApplicationRequest request, CancellationToken cancellationToken)
    {
        var deployment = await Find(id, cancellationToken);
        deployment.Scale(request.Replicas);
        await gateway.ScaleDeploymentAsync(deployment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RestartAsync(Guid id, CancellationToken cancellationToken)
    {
        var deployment = await Find(id, cancellationToken);
        deployment.MarkRestarted();
        await gateway.RestartDeploymentAsync(deployment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var deployment = await Find(id, cancellationToken);
        await gateway.DeleteDeploymentAsync(deployment, cancellationToken);
        repository.Remove(deployment);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Deployment> Find(Guid id, CancellationToken cancellationToken)
        => await repository.GetAsync(id, cancellationToken) ?? throw new KeyNotFoundException("Application was not found.");

    private static ApplicationSummary Map(Deployment x) => new(x.Id, x.Name, x.Namespace, x.Image, x.Status.ToString(), x.Replicas, x.AvailableReplicas, x.Port, x.IngressHost, x.CreatedAt, x.UpdatedAt);
}

public sealed class EnvironmentService(
    IEnvironmentRepository repository,
    IUnitOfWork unitOfWork,
    IPlatformGateway gateway) : IEnvironmentService
{
    public async Task<IReadOnlyList<EnvironmentSummary>> ListAsync(CancellationToken cancellationToken)
        => (await repository.ListAsync(cancellationToken)).Select(Map).ToArray();

    public async Task<EnvironmentSummary> CreateAsync(CreateEnvironmentRequest request, string owner, CancellationToken cancellationToken)
    {
        var environment = new Vertex.Domain.Entities.Environment(Guid.NewGuid(), request.Name, request.Namespace, owner);
        await repository.AddAsync(environment, cancellationToken);
        await gateway.CreateEnvironmentAsync(environment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(environment);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var environment = (await repository.ListAsync(cancellationToken)).FirstOrDefault(x => x.Id == id)
            ?? throw new KeyNotFoundException("Environment was not found.");
        await gateway.DeleteEnvironmentAsync(environment, cancellationToken);
        repository.Remove(environment);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static EnvironmentSummary Map(Vertex.Domain.Entities.Environment x) => new(x.Id, x.Name, x.Namespace, x.Owner, x.Status.ToString(), x.CreatedAt);
}

public sealed class SecretService(ISecretRepository repository, IUnitOfWork unitOfWork) : ISecretService
{
    public async Task<IReadOnlyList<SecretSummary>> ListAsync(CancellationToken cancellationToken)
        => (await repository.ListAsync(cancellationToken)).Select(x => new SecretSummary(x.Id, x.Name, x.Namespace, x.Values.Count, x.UpdatedAt)).ToArray();

    public async Task<SecretDetail?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var secret = await repository.GetAsync(id, cancellationToken);
        return secret is null ? null : Map(secret);
    }

    public async Task<SecretDetail> UpsertAsync(Guid? id, UpsertSecretRequest request, CancellationToken cancellationToken)
    {
        var secret = id.HasValue ? await repository.GetAsync(id.Value, cancellationToken) : null;
        if (secret is null)
        {
            secret = new Secret(Guid.NewGuid(), request.Name, request.Namespace, request.Values);
            await repository.AddAsync(secret, cancellationToken);
        }
        else
        {
            secret.Update(request.Values);
        }
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(secret);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var secret = await repository.GetAsync(id, cancellationToken) ?? throw new KeyNotFoundException("Secret was not found.");
        repository.Remove(secret);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static SecretDetail Map(Secret x) => new(x.Id, x.Name, x.Namespace, new Dictionary<string, string>(x.Values), x.UpdatedAt);
}

public sealed class DatabaseService(
    IDatabaseRepository repository,
    IUnitOfWork unitOfWork,
    IPlatformGateway gateway) : IDatabaseService
{
    public async Task<IReadOnlyList<DatabaseSummary>> ListAsync(CancellationToken cancellationToken)
        => (await repository.ListAsync(cancellationToken)).Select(Map).ToArray();

    public async Task<DatabaseSummary> CreateAsync(CreateDatabaseRequest request, CancellationToken cancellationToken)
    {
        var database = new Database(Guid.NewGuid(), request.Name, request.Namespace, $"{request.Name}-postgres.{request.Namespace}.svc.cluster.local", 5432, "vertex", "vertex-demo-password");
        await repository.AddAsync(database, cancellationToken);
        await gateway.ProvisionDatabaseAsync(database, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(database);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var database = (await repository.ListAsync(cancellationToken)).FirstOrDefault(x => x.Id == id)
            ?? throw new KeyNotFoundException("Database was not found.");
        await gateway.DeleteDatabaseAsync(database, cancellationToken);
        repository.Remove(database);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static DatabaseSummary Map(Database x) => new(x.Id, x.Name, x.Namespace, x.Host, x.Port, x.Username, x.Password, x.ConnectionString, x.Status.ToString(), x.CreatedAt);
}

public sealed class LogService(IPlatformGateway gateway) : ILogService
{
    public async Task<LogsResponse> GetAsync(string application, string? pod, CancellationToken cancellationToken)
    {
        var resolvedPod = string.IsNullOrWhiteSpace(pod) ? $"{application}-7c8bd9b9f8-x2k4m" : pod;
        return new LogsResponse(application, resolvedPod, await gateway.GetLogsAsync(application, resolvedPod, cancellationToken));
    }
}

