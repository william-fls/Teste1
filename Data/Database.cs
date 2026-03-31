using Npgsql;

namespace CoworkingAgendamento.Data;

public static class Database
{
    private const string ConnectionStringVariableName = "COWORKING_CONNECTION_STRING";
    private const string LocalConnectionFileName = "connection-string.txt";

    private static string ConnectionString =>
        GetConfiguredConnectionString();

    public static NpgsqlConnection GetConnection()
    {
        var conn = new NpgsqlConnection(ConnectionString);
        conn.Open();
        return conn;
    }

    private static string GetConfiguredConnectionString()
    {
        var envConnectionString = Environment.GetEnvironmentVariable(ConnectionStringVariableName);
        if (!string.IsNullOrWhiteSpace(envConnectionString))
            return envConnectionString;

        foreach (var path in GetCandidatePaths())
        {
            if (!File.Exists(path))
                continue;

            var fileConnectionString = File.ReadAllText(path).Trim();
            if (!string.IsNullOrWhiteSpace(fileConnectionString))
                return fileConnectionString;
        }

        throw new InvalidOperationException(
            $"Defina a variavel de ambiente {ConnectionStringVariableName} ou crie o arquivo " +
            $"{LocalConnectionFileName} ao lado do executavel/projeto com a connection string do PostgreSQL antes de executar a aplicacao.");
    }

    private static IEnumerable<string> GetCandidatePaths()
    {
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var rootPath in new[] { AppContext.BaseDirectory, Environment.CurrentDirectory })
        {
            if (string.IsNullOrWhiteSpace(rootPath))
                continue;

            var directory = new DirectoryInfo(rootPath);
            while (directory is not null)
            {
                var path = Path.Combine(directory.FullName, LocalConnectionFileName);
                if (paths.Add(path))
                    yield return path;

                directory = directory.Parent;
            }
        }
    }
}
