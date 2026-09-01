using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DominoPontaDeQuina.Domain.Entities;

[Table("TimesPartida")]
public class TimePartida
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid PartidaId { get; set; }

    public Partida Partida { get; set; } = null!;

    [Required]
    [MaxLength(60)]
    public string Nome { get; set; } = string.Empty;

    public int Pontuacao { get; set; }

    public bool Vencedor { get; set; }

    public ICollection<ParticipacaoPartida> Participacoes { get; set; } = new List<ParticipacaoPartida>();
}
