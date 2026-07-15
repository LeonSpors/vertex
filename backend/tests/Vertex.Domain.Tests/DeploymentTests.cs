using Vertex.Domain.Entities;
using Xunit;

namespace Vertex.Domain.Tests;

public sealed class DeploymentTests
{
    [Fact]
    public void Scale_updates_desired_and_available_replicas()
    {
        var deployment = new Deployment(Guid.NewGuid(), "checkout-api", "production", "ghcr.io/vertex/checkout:v1", 2, 8080, null);
        deployment.Scale(5);
        Assert.Equal(5, deployment.Replicas);
        Assert.Equal(5, deployment.AvailableReplicas);
    }

    [Fact]
    public void Scale_rejects_unbounded_replica_counts()
    {
        var deployment = new Deployment(Guid.NewGuid(), "checkout-api", "production", "ghcr.io/vertex/checkout:v1", 2, 8080, null);
        Assert.Throws<ArgumentOutOfRangeException>(() => deployment.Scale(101));
    }
}
