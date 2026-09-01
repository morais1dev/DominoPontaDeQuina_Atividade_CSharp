using DominoPontaDeQuina.Services.Models;

namespace DominoPontaDeQuina.Services.Interfaces;

/// <summary>
/// Define as regras de uso das consultas de desempenho dos jogadores.
/// As contagens e somatorias sao delegadas aos repositorios, que concentram as expressoes LINQ.
/// </summary>
public interface IEstatisticasService
{
    /// <summary>
    /// Obtem o desempenho acumulado do jogador informado.
    /// </summary>
    /// <param name="jogadorId">O identificador do jogador.</param>
    /// <returns>As estatisticas do jogador.</returns>
    Task<EstatisticasJogador> ObterDoJogadorAsync(Guid jogadorId);

    /// <summary>
    /// Monta o ranking dos jogadores ordenado por vitorias e, em caso de empate, por pontuacao.
    /// </summary>
    /// <param name="quantidade">A quantidade maxima de jogadores retornados.</param>
    /// <returns>O ranking dos jogadores.</returns>
    Task<IReadOnlyList<EstatisticasJogador>> ObterRankingAsync(int quantidade = 10);
}
