using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DominoPontaDeQuina.Domain.Entities;

[Table("ParticipacoesPartida")]
public class ParticipacaoPartida
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid PartidaId { get; set; }

    public Partida Partida { get; set; } = null!;

    [Required]
    public Guid JogadorId { get; set; }

    public Jogador Jogador { get; set; } = null!;

    public Guid? TimePartidaId { get; set; }

    public TimePartida? Time { get; set; }

    public int Posicao { get; set; }

    public int Pontuacao { get; set; }

    public bool Vencedor { get; set; }
}
