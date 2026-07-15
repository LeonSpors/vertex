using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vertex.Application.Abstractions;
using Vertex.Domain.Entities;
using Vertex.Infrastructure.Auth;
using Vertex.Infrastructure.Kubernetes;
using Vertex.Infrastructure.Persistence;

namespace Vertex.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddVertexInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var useInMemory = configuration.GetValue("Database:UseInMemory", true);
        if (useInMemory)
        {
            services.AddDbContext<VertexDbContext>(options => options.UseInMemoryDatabase("vertex-local"));
        }
        else
        {
            services.AddDbContext<VertexDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("Postgres")));
        }

        services.Configure<KubernetesOptions>(configuration.GetSection("Kubernetes"));
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.AddSingleton<KubernetesQuantityParser>();
        services.AddSingleton<KubernetesClientProvider>();
        services.AddSingleton<IClusterSetupService>(provider => provider.GetRequiredService<KubernetesClientProvider>());
        services.AddScoped<KubernetesMetricsReader>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<DatabaseInitializer>();
        services.AddScoped<IDeploymentRepository, DeploymentRepository>();
        services.AddScoped<IEnvironmentRepository, EnvironmentRepository>();
        services.AddScoped<ISecretRepository, SecretRepository>();
        services.AddScoped<IDatabaseRepository, DatabaseRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPlatformGateway, KubernetesPlatformGateway>();
        services.AddScoped<ITokenService, JwtTokenService>();
        return services;
    }
}
