using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DominoPontaDeQuina.Domain.Enums;

namespace DominoPontaDeQuina.Domain.Entities;

[Table("Partidas")]
public class Partida
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [Range(1, int.MaxValue)]
    public int PontuacaoAlvo { get; set; } = 50;

    [Required]
    public DateTime IniciadoEm { get; set; } = DateTime.UtcNow;

    public DateTime? FinalizadoEm { get; set; }

    [Required]
    public StatusPartida Status { get; set; } = StatusPartida.Aguardando;

    public ICollection<TimePartida> Times { get; set; } = new List<TimePartida>();

    public ICollection<ParticipacaoPartida> Participacoes { get; set; } = new List<ParticipacaoPartida>();

    public ICollection<Rodada> Rodadas { get; set; } = new List<Rodada>();
}
