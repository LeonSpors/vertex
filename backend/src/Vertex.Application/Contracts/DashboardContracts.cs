namespace Vertex.Application.Contracts;

public sealed record DashboardResponse(
    ClusterSummary Cluster,
    ResourceSummary Resources,
    IReadOnlyList<NodeSummary> Nodes,
    IReadOnlyList<EventSummary> RecentEvents);

public sealed record ClusterSummary(string Status, string Version, int Namespaces, int StorageClasses, int IngressClasses);
public sealed record ResourceSummary(int RunningPods, int RunningDeployments, int Nodes, double? CpuUsagePercent, double? MemoryUsagePercent);
public sealed record NodeSummary(string Name, string Status, string Cpu, string Memory, string Role);
public sealed record EventSummary(string Type, string Reason, string Message, string Namespace, DateTimeOffset Timestamp);

public sealed record ClusterSetupResponse(
    string Status,
    string Mode,
    string? Version,
    string? Error,
    IReadOnlyList<ClusterSetupStep> Steps);

public sealed record ClusterSetupStep(int Number, string Title, string Description, string Command);
