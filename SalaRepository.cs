using CoworkingAgendamento.Data;
using CoworkingAgendamento.Models;
using Npgsql;

namespace CoworkingAgendamento.Repositories;

public class SalaRepository
{
    public List<Sala> ListarTodas()
    {
        var salas = new List<Sala>();
        using var conn = Database.GetConnection();
        using var cmd = new NpgsqlCommand("SELECT id, nome FROM sala ORDER BY nome", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            salas.Add(new Sala { Id = reader.GetInt32(0), Nome = reader.GetString(1) });
        return salas;
    }

    public void Inserir(Sala sala)
    {
        using var conn = Database.GetConnection();
        using var cmd = new NpgsqlCommand(
            "INSERT INTO sala (nome) VALUES (@nome)", conn);
        cmd.Parameters.AddWithValue("nome", sala.Nome.Trim());
        cmd.ExecuteNonQuery();
    }

    public void Atualizar(Sala sala)
    {
        using var conn = Database.GetConnection();
        using var cmd = new NpgsqlCommand(
            "UPDATE sala SET nome = @nome WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("nome", sala.Nome.Trim());
        cmd.Parameters.AddWithValue("id", sala.Id);
        cmd.ExecuteNonQuery();
    }

    public void Excluir(int id)
    {
        using var conn = Database.GetConnection();
        using var cmd = new NpgsqlCommand("DELETE FROM sala WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        cmd.ExecuteNonQuery();
    }
}
