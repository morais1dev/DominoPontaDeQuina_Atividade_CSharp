using DominoPontaDeQuina.Domain.Entities;

namespace DominoPontaDeQuina.Repository.Interfaces;

/// <summary>
/// Define as consultas de persistencia disponiveis para o vinculo entre jogador e partida.
/// </summary>
public interface IParticipacaoPartidaRepository : IRepositorioBase<ParticipacaoPartida>
{
    /// <summary>
    /// Lista as participacoes da partida informada em ordem de posicao na mesa.
    /// </summary>
    /// <param name="partidaId">O identificador da partida.</param>
    /// <returns>As participacoes da partida.</returns>
    Task<List<ParticipacaoPartida>> ListarPorPartidaAsync(Guid partidaId);

    /// <summary>
    /// Lista as participacoes do jogador informado, da partida mais recente para a mais antiga.
    /// </summary>
    /// <param name="jogadorId">O identificador do jogador.</param>
    /// <returns>As participacoes do jogador.</returns>
    Task<List<ParticipacaoPartida>> ListarPorJogadorAsync(Guid jogadorId);

    /// <summary>
    /// Obtem a participacao vencedora da partida informada.
    /// </summary>
    /// <param name="partidaId">O identificador da partida.</param>
    /// <returns>A participacao vencedora, ou <see langword="null"/> quando ainda nao houver vencedor.</returns>
    Task<ParticipacaoPartida?> ObterVencedorDaPartidaAsync(Guid partidaId);

    /// <summary>
    /// Conta quantas partidas o jogador informado venceu.
    /// </summary>
    /// <param name="jogadorId">O identificador do jogador.</param>
    /// <returns>A quantidade de vitorias.</returns>
    Task<int> ContarVitoriasDoJogadorAsync(Guid jogadorId);

    /// <summary>
    /// Conta quantas partidas o jogador informado disputou.
    /// </summary>
    /// <param name="jogadorId">O identificador do jogador.</param>
    /// <returns>A quantidade de partidas disputadas.</returns>
    Task<int> ContarPartidasDoJogadorAsync(Guid jogadorId);

    /// <summary>
    /// Soma a pontuacao acumulada pelo jogador informado em todas as suas participacoes.
    /// </summary>
    /// <param name="jogadorId">O identificador do jogador.</param>
    /// <returns>A pontuacao total do jogador.</returns>
    Task<int> SomarPontuacaoDoJogadorAsync(Guid jogadorId);
}
