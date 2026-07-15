using Microsoft.Extensions.DependencyInjection;
using Vertex.Application.Abstractions;
using Vertex.Application.Services;

namespace Vertex.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddVertexApplication(this IServiceCollection services)
    {
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IApplicationService, ApplicationService>();
        services.AddScoped<IEnvironmentService, EnvironmentService>();
        services.AddScoped<ISecretService, SecretService>();
        services.AddScoped<IDatabaseService, DatabaseService>();
        services.AddScoped<ILogService, LogService>();
        return services;
    }
}

