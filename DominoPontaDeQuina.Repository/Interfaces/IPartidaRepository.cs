using DominoPontaDeQuina.Domain.Entities;
using DominoPontaDeQuina.Domain.Enums;

namespace DominoPontaDeQuina.Repository.Interfaces;

/// <summary>
/// Define as consultas de persistencia disponiveis para a partida armazenada.
/// </summary>
public interface IPartidaRepository : IRepositorioBase<Partida>
{
    /// <summary>
    /// Obtem a partida com times, participacoes, rodadas e jogadas carregados.
    /// </summary>
    /// <param name="id">O identificador da partida.</param>
    /// <returns>A partida completa, ou <see langword="null"/> quando nao existir.</returns>
    Task<Partida?> ObterCompletaPorIdAsync(Guid id);

    /// <summary>
    /// Lista as partidas que estejam no status informado.
    /// </summary>
    /// <param name="status">O status pesquisado.</param>
    /// <returns>As partidas encontradas, da mais recente para a mais antiga.</returns>
    Task<List<Partida>> ListarPorStatusAsync(StatusPartida status);

    /// <summary>
    /// Lista as partidas em que o jogador informado participou.
    /// </summary>
    /// <param name="jogadorId">O identificador do jogador.</param>
    /// <returns>As partidas encontradas, da mais recente para a mais antiga.</returns>
    Task<List<Partida>> ListarPorJogadorAsync(Guid jogadorId);

    /// <summary>
    /// Lista as ultimas partidas registradas.
    /// </summary>
    /// <param name="quantidade">A quantidade maxima de partidas retornadas.</param>
    /// <returns>As partidas mais recentes.</returns>
    Task<List<Partida>> ListarUltimasAsync(int quantidade);
}
