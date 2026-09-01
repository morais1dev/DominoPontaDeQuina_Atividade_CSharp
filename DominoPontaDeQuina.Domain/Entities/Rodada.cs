using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DominoPontaDeQuina.Domain.Enums;

namespace DominoPontaDeQuina.Domain.Entities;

[Table("Rodadas")]
public class Rodada
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid PartidaId { get; set; }

    public Partida Partida { get; set; } = null!;

    [Required]
    [Range(1, int.MaxValue)]
    public int Numero { get; set; }

    [Required]
    public StatusRodada Status { get; set; } = StatusRodada.EmAndamento;

    public TipoFinalizacaoRodada? TipoFinalizacao { get; set; }

    public Guid? JogadorVencedorId { get; set; }

    public Jogador? JogadorVencedor { get; set; }

    public int PontuacaoVencedor { get; set; }

    [Required]
    public DateTime IniciadaEm { get; set; } = DateTime.UtcNow;

    public DateTime? FinalizadaEm { get; set; }

    public ICollection<Jogada> Jogadas { get; set; } = new List<Jogada>();
}
