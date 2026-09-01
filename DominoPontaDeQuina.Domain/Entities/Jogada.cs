using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DominoPontaDeQuina.Domain.Enums;

namespace DominoPontaDeQuina.Domain.Entities;

[Table("Jogadas")]
public class Jogada
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid RodadaId { get; set; }

    public Rodada Rodada { get; set; } = null!;

    [Required]
    public Guid JogadorId { get; set; }

    public Jogador Jogador { get; set; } = null!;

    [Required]
    [Range(1, int.MaxValue)]
    public int Sequencia { get; set; }

    [Range(0, 6)]
    public int? PecaValorA { get; set; }

    [Range(0, 6)]
    public int? PecaValorB { get; set; }

    public LadoTabuleiro? Lado { get; set; }

    public bool PassouVez { get; set; }

    public int PontosGerados { get; set; }

    [Required]
    public DateTime RegistradaEm { get; set; } = DateTime.UtcNow;
}
