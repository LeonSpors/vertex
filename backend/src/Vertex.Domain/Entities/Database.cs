namespace Vertex.Domain.Entities;

public sealed class Database
{
    private Database() { }

    public Database(Guid id, string name, string @namespace, string host, int port, string username, string password)
    {
        Id = id;
        Name = name;
        Namespace = @namespace;
        Host = host;
        Port = port;
        Username = username;
        Password = password;
        Status = DatabaseStatus.Ready;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Namespace { get; private set; } = string.Empty;
    public string Host { get; private set; } = string.Empty;
    public int Port { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string Password { get; private set; } = string.Empty;
    public DatabaseStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public string ConnectionString => $"Host={Host};Port={Port};Database={Name};Username={Username};Password={Password}";
}

public enum DatabaseStatus { Ready, Provisioning, Failed, Deleting }

