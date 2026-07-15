using Vertex.Domain.Entities;

namespace Vertex.Application.Abstractions;

public interface IDeploymentRepository
{
    Task<IReadOnlyList<Deployment>> ListAsync(CancellationToken cancellationToken);
    Task<Deployment?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(Deployment deployment, CancellationToken cancellationToken);
    void Remove(Deployment deployment);
}

public interface IEnvironmentRepository
{
    Task<IReadOnlyList<Vertex.Domain.Entities.Environment>> ListAsync(CancellationToken cancellationToken);
    Task AddAsync(Vertex.Domain.Entities.Environment environment, CancellationToken cancellationToken);
    void Remove(Vertex.Domain.Entities.Environment environment);
}

public interface ISecretRepository
{
    Task<IReadOnlyList<Secret>> ListAsync(CancellationToken cancellationToken);
    Task<Secret?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(Secret secret, CancellationToken cancellationToken);
    void Remove(Secret secret);
}

public interface IDatabaseRepository
{
    Task<IReadOnlyList<Database>> ListAsync(CancellationToken cancellationToken);
    Task AddAsync(Database database, CancellationToken cancellationToken);
    void Remove(Database database);
}

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken);
}

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
