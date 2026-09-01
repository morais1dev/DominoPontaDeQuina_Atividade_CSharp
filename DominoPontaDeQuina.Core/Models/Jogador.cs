namespace DominoPontaDeQuina.Core.Models;

/// <summary>
/// Representa o jogador na hierarquia Partida -> Rodadas -> Jogadas.
/// Neste nivel ficam apenas a identidade e os dados persistentes do participante.
/// As pecas usadas durante a partida ficam separadas em <see cref="MaoJogador"/>.
/// </summary>
/// <param name="nome">O nome do jogador.</param>
/// <param name="id">O identificador do jogador, usado para reaproveitar a identidade ja persistida.</param>
public class Jogador(string nome, Guid? id = null)
{
    /// <summary>
    /// Obtem o identificador unico do jogador.
    /// Quando a partida e montada a partir de dados persistidos, este identificador reaproveita o valor de origem.
    /// </summary>
    public Guid Id { get; } = id ?? Guid.NewGuid();

    /// <summary>
    /// Obtem o nome exibido do jogador na partida.
    /// </summary>
    public string Nome { get; } = nome;
}