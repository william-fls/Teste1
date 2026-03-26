namespace CoworkingAgendamento.Models;

public class Sala
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;

    public override string ToString() => Nome;
}
