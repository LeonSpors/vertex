namespace Vertex.Application.Abstractions;

public interface ICurrentUser
{
    string? Email { get; }

    bool CanAccessNamespace(string @namespace);

    void DemandNamespace(string @namespace);
}
