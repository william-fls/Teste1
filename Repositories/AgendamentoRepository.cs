using CoworkingAgendamento.Data;
using CoworkingAgendamento.Models;
using Npgsql;
using NpgsqlTypes;

namespace CoworkingAgendamento.Repositories;

public class AgendamentoRepository
{
    public List<Agendamento> ListarTodos()
    {
        var lista = new List<Agendamento>();
        using var conn = Database.GetConnection();
        const string sql = @"
            SELECT a.id, a.sala_id, s.nome, a.inicio, a.fim
            FROM agendamento a
            JOIN sala s ON s.id = a.sala_id
            ORDER BY a.inicio";
        using var cmd = new NpgsqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            lista.Add(new Agendamento
            {
                Id       = reader.GetInt32(0),
                SalaId   = reader.GetInt32(1),
                SalaNome = reader.GetString(2),
                Inicio   = reader.GetDateTime(3),
                Fim      = reader.GetDateTime(4)
            });
        return lista;
    }

    public void Inserir(Agendamento a)
    {
        using var conn = Database.GetConnection();
        using var cmd = new NpgsqlCommand(
            "INSERT INTO agendamento (sala_id, inicio, fim) VALUES (@sala, @ini, @fim)", conn);
        AddSalaIdParameter(cmd, a.SalaId);
        cmd.Parameters.AddWithValue("ini",  a.Inicio);
        cmd.Parameters.AddWithValue("fim",  a.Fim);
        cmd.ExecuteNonQuery();
    }

    public void Atualizar(Agendamento a)
    {
        using var conn = Database.GetConnection();
        using var cmd = new NpgsqlCommand(
            "UPDATE agendamento SET sala_id=@sala, inicio=@ini, fim=@fim WHERE id=@id", conn);
        AddSalaIdParameter(cmd, a.SalaId);
        cmd.Parameters.AddWithValue("ini",  a.Inicio);
        cmd.Parameters.AddWithValue("fim",  a.Fim);
        cmd.Parameters.AddWithValue("id",   a.Id);
        cmd.ExecuteNonQuery();
    }

    public bool PossuiPeriodoFuturo(int id)
    {
        using var conn = Database.GetConnection();
        using var cmd = new NpgsqlCommand(
            """
            SELECT EXISTS (
                SELECT 1
                FROM agendamento
                WHERE id = @id
                  AND fim > NOW()
            )
            """, conn);
        cmd.Parameters.AddWithValue("id", id);
        return (bool)(cmd.ExecuteScalar() ?? false);
    }

    public void Excluir(int id)
    {
        using var conn = Database.GetConnection();
        using var cmd = new NpgsqlCommand("DELETE FROM agendamento WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        cmd.ExecuteNonQuery();
    }

    private static void AddSalaIdParameter(NpgsqlCommand cmd, int? salaId)
    {
        cmd.Parameters.Add("sala", NpgsqlDbType.Integer).Value =
            salaId is int value ? value : DBNull.Value;
    }
}
