using DominoPontaDeQuina.Domain.Entities;

namespace DominoPontaDeQuina.Repository.Interfaces;

/// <summary>
/// Define as consultas de persistencia disponiveis para as jogadas registradas em uma rodada.
/// </summary>
public interface IJogadaRepository : IRepositorioBase<Jogada>
{
    /// <summary>
    /// Adiciona varias jogadas em uma unica confirmacao no banco de dados.
    /// </summary>
    /// <param name="jogadas">As jogadas a serem persistidas.</param>
    Task AdicionarVariasAsync(IEnumerable<Jogada> jogadas);

    /// <summary>
    /// Lista as jogadas da rodada informada na ordem em que foram executadas.
    /// </summary>
    /// <param name="rodadaId">O identificador da rodada.</param>
    /// <returns>As jogadas da rodada.</returns>
    Task<List<Jogada>> ListarPorRodadaAsync(Guid rodadaId);

    /// <summary>
    /// Conta quantas jogadas o jogador informado ja executou.
    /// </summary>
    /// <param name="jogadorId">O identificador do jogador.</param>
    /// <returns>A quantidade de jogadas do jogador.</returns>
    Task<int> ContarPorJogadorAsync(Guid jogadorId);

    /// <summary>
    /// Conta quantas vezes o jogador informado precisou passar a vez.
    /// </summary>
    /// <param name="jogadorId">O identificador do jogador.</param>
    /// <returns>A quantidade de passagens de vez do jogador.</returns>
    Task<int> ContarPassesDoJogadorAsync(Guid jogadorId);

    /// <summary>
    /// Soma os pontos gerados pelo jogador informado em todas as suas jogadas.
    /// </summary>
    /// <param name="jogadorId">O identificador do jogador.</param>
    /// <returns>O total de pontos gerados pelo jogador.</returns>
    Task<int> SomarPontosDoJogadorAsync(Guid jogadorId);
}
