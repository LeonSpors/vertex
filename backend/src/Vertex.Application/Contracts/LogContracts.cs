namespace Vertex.Application.Contracts;

public sealed record LogLine(string Timestamp, string Level, string Message);
public sealed record LogsResponse(string Application, string Pod, IReadOnlyList<LogLine> Lines);

