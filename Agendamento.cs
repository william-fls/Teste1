namespace CoworkingAgendamento.Models;

public class Agendamento
{
    public int Id { get; set; }
    public int SalaId { get; set; }
    public string SalaNome { get; set; } = string.Empty;
    public DateTime Inicio { get; set; }
    public DateTime Fim { get; set; }
}
