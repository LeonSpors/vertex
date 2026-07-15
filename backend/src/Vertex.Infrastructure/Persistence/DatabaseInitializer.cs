using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Vertex.Domain.Entities;

namespace Vertex.Infrastructure.Persistence;

public sealed class DatabaseInitializer(VertexDbContext db, IPasswordHasher<User> passwordHasher, ILogger<DatabaseInitializer> logger)
{
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await db.Database.EnsureCreatedAsync(cancellationToken);
        if (await db.Users.AnyAsync(cancellationToken)) return;

        var user = new User(Guid.NewGuid(), "admin@vertex.local", "Alex Morgan", string.Empty);
        var hash = passwordHasher.HashPassword(user, "vertex-dev");
        var userEntry = db.Entry(user);
        userEntry.Property(x => x.PasswordHash).CurrentValue = hash;
        db.Users.Add(user);

        db.Deployments.AddRange(
            new Deployment(Guid.NewGuid(), "checkout-api", "production", "ghcr.io/vertex/checkout-api:v1.8.2", 3, 8080, "checkout.vertex.local"),
            new Deployment(Guid.NewGuid(), "catalog-worker", "production", "ghcr.io/vertex/catalog-worker:v2.4.0", 2, 8080, null),
            new Deployment(Guid.NewGuid(), "docs-site", "staging", "ghcr.io/vertex/docs-site:v0.9.1", 1, 3000, "docs.staging.vertex.local"));
        db.Environments.AddRange(
            new Vertex.Domain.Entities.Environment(Guid.NewGuid(), "Production", "production", "platform@vertex.local"),
            new Vertex.Domain.Entities.Environment(Guid.NewGuid(), "Staging", "staging", "alex@vertex.local"),
            new Vertex.Domain.Entities.Environment(Guid.NewGuid(), "Preview / PR-482", "preview-pr-482", "maya@vertex.local"));
        db.Secrets.AddRange(
            new Secret(Guid.NewGuid(), "checkout-config", "production", new Dictionary<string, string> { ["DATABASE_URL"] = "postgres://••••", ["STRIPE_KEY"] = "sk_live_••••" }),
            new Secret(Guid.NewGuid(), "catalog-config", "production", new Dictionary<string, string> { ["REDIS_URL"] = "redis://••••", ["SEARCH_TOKEN"] = "••••" }),
            new Secret(Guid.NewGuid(), "preview-env", "preview-pr-482", new Dictionary<string, string> { ["FEATURE_FLAG"] = "new-checkout" }));
        db.Databases.Add(new Database(Guid.NewGuid(), "checkout", "production", "checkout-postgres.production.svc.cluster.local", 5432, "vertex", "vertex-demo-password"));

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded Vertex local demo workspace");
    }
}
