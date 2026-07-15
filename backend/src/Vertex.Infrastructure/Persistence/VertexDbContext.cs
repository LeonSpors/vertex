using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Vertex.Domain.Entities;

namespace Vertex.Infrastructure.Persistence;

public sealed class VertexDbContext(DbContextOptions<VertexDbContext> options) : DbContext(options)
{
    public DbSet<Deployment> Deployments => Set<Deployment>();
    public DbSet<Vertex.Domain.Entities.Environment> Environments => Set<Vertex.Domain.Entities.Environment>();
    public DbSet<Secret> Secrets => Set<Secret>();
    public DbSet<Database> Databases => Set<Database>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Deployment>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(63).IsRequired();
            entity.Property(x => x.Namespace).HasMaxLength(63).IsRequired();
            entity.Property(x => x.Image).HasMaxLength(300).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
            entity.HasIndex(x => new { x.Namespace, x.Name }).IsUnique();
        });
        modelBuilder.Entity<Vertex.Domain.Entities.Environment>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Namespace).HasMaxLength(63).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
            entity.HasIndex(x => x.Namespace).IsUnique();
        });
        modelBuilder.Entity<Secret>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(63).IsRequired();
            entity.Property(x => x.Namespace).HasMaxLength(63).IsRequired();
            var values = entity.Property(x => x.Values).HasConversion(new ValueConverter<Dictionary<string, string>, string>(
                value => System.Text.Json.JsonSerializer.Serialize(value, (System.Text.Json.JsonSerializerOptions?)null),
                value => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(value, (System.Text.Json.JsonSerializerOptions?)null) ?? new()));
            values.Metadata.SetValueComparer(new ValueComparer<Dictionary<string, string>>(
                (left, right) => left != null && right != null && left.SequenceEqual(right),
                value => value.Aggregate(0, (hash, pair) => HashCode.Combine(hash, pair.Key, pair.Value)),
                value => new Dictionary<string, string>(value, StringComparer.Ordinal)));
            entity.HasIndex(x => new { x.Namespace, x.Name }).IsUnique();
        });
        modelBuilder.Entity<Database>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Namespace).HasMaxLength(63).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
            entity.Ignore(x => x.ConnectionString);
            entity.HasIndex(x => new { x.Namespace, x.Name }).IsUnique();
        });
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Email).HasMaxLength(320).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.PasswordHash).IsRequired();
            entity.HasIndex(x => x.Email).IsUnique();
        });
    }
}
