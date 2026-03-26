using System.IO;
using Npgsql;

namespace CoworkingAgendamento.Data;

public static class Database
{
    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("COWORKING_CONNECTION_STRING")
        ?? "Host=localhost;Port=5432;Database=coworking;Username=postgres;Password=capivara";

    public static NpgsqlConnection GetConnection()
    {
        var conn = new NpgsqlConnection(ConnectionString);
        conn.Open();
        return conn;
    }
}
