using Microsoft.EntityFrameworkCore;
using Vertex.Application.Abstractions;
using Vertex.Domain.Entities;

namespace Vertex.Infrastructure.Persistence;

public sealed class DeploymentRepository(VertexDbContext db) : IDeploymentRepository
{
    public async Task<IReadOnlyList<Deployment>> ListAsync(CancellationToken cancellationToken) => await db.Deployments.AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
    public Task<Deployment?> GetAsync(Guid id, CancellationToken cancellationToken) => db.Deployments.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public Task AddAsync(Deployment deployment, CancellationToken cancellationToken) => db.Deployments.AddAsync(deployment, cancellationToken).AsTask();
    public void Remove(Deployment deployment) => db.Deployments.Remove(deployment);
}

public sealed class EnvironmentRepository(VertexDbContext db) : IEnvironmentRepository
{
    public async Task<IReadOnlyList<Vertex.Domain.Entities.Environment>> ListAsync(CancellationToken cancellationToken) => await db.Environments.AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
    public Task AddAsync(Vertex.Domain.Entities.Environment environment, CancellationToken cancellationToken) => db.Environments.AddAsync(environment, cancellationToken).AsTask();
    public void Remove(Vertex.Domain.Entities.Environment environment) => db.Environments.Remove(environment);
}

public sealed class SecretRepository(VertexDbContext db) : ISecretRepository
{
    public async Task<IReadOnlyList<Secret>> ListAsync(CancellationToken cancellationToken) => await db.Secrets.AsNoTracking().OrderBy(x => x.Namespace).ThenBy(x => x.Name).ToListAsync(cancellationToken);
    public Task<Secret?> GetAsync(Guid id, CancellationToken cancellationToken) => db.Secrets.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public Task AddAsync(Secret secret, CancellationToken cancellationToken) => db.Secrets.AddAsync(secret, cancellationToken).AsTask();
    public void Remove(Secret secret) => db.Secrets.Remove(secret);
}

public sealed class DatabaseRepository(VertexDbContext db) : IDatabaseRepository
{
    public async Task<IReadOnlyList<Database>> ListAsync(CancellationToken cancellationToken) => await db.Databases.AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
    public Task AddAsync(Database database, CancellationToken cancellationToken) => db.Databases.AddAsync(database, cancellationToken).AsTask();
    public void Remove(Database database) => db.Databases.Remove(database);
}

public sealed class UserRepository(VertexDbContext db) : IUserRepository
{
    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken) => db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Email == email.ToLowerInvariant(), cancellationToken);
}

public sealed class UnitOfWork(VertexDbContext db) : IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}

