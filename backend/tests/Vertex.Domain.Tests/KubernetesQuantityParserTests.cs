using Vertex.Infrastructure.Kubernetes;
using Xunit;

namespace Vertex.Domain.Tests;

public sealed class KubernetesQuantityParserTests
{
    private readonly KubernetesQuantityParser parser = new();

    [Fact]
    public void Parses_cpu_quantities_to_nano_cores()
    {
        Assert.Equal(500_000_000, parser.ParseCpu("500m"));
        Assert.Equal(125_000_000, parser.ParseCpu("125000000n"));
    }

    [Fact]
    public void Parses_binary_memory_quantities_to_bytes()
    {
        Assert.Equal(2 * 1024d * 1024d * 1024d, parser.ParseMemory("2Gi"));
    }
}
