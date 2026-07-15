using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Vertex.Infrastructure.Kubernetes;
using Xunit;

namespace Vertex.Domain.Tests;

public sealed class KubernetesClientProviderTests
{
    [Fact]
    public async Task Missing_kubeconfig_reports_assisted_setup_without_throwing()
    {
        using var provider = new KubernetesClientProvider(
            Options.Create(new KubernetesOptions
            {
                Mode = "Cluster",
                KubeConfigPath = Path.Combine(Path.GetTempPath(), $"vertex-missing-{Guid.NewGuid():N}.yaml")
            }),
            NullLogger<KubernetesClientProvider>.Instance);

        var setup = await provider.GetAsync(CancellationToken.None);

        Assert.Equal("SetupRequired", setup.Status);
        Assert.Equal("Cluster", setup.Mode);
        Assert.Equal(4, setup.Steps.Count);
        Assert.NotNull(setup.Error);
    }
}
