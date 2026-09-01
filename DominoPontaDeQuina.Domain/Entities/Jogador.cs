using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DominoPontaDeQuina.Domain.Entities;

[Table("Jogadores")]
public class Jogador
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(80)]
    public string NomeExibicao { get; set; } = string.Empty;

    [Required]
    public Guid UsuarioId { get; set; }

    public Usuario Usuario { get; set; } = null!;

    [Required]
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public ICollection<ParticipacaoPartida> Participacoes { get; set; } = new List<ParticipacaoPartida>();

    public ICollection<Jogada> Jogadas { get; set; } = new List<Jogada>();

    public ICollection<Rodada> RodadasVencidas { get; set; } = new List<Rodada>();
}
