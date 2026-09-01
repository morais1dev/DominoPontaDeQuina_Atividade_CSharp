using DominoPontaDeQuina.Core;
using DominoPontaDeQuina.Domain.Entities;
using DominoPontaDeQuina.Services.Models;

namespace DominoPontaDeQuina.Services.Interfaces;

/// <summary>
/// Define as regras de uso de uma partida, da montagem da mesa ate a persistencia do resultado.
/// Este servico e a fronteira entre o motor do jogo, que vive em memoria, e o modelo persistente
/// composto por partida, times, participacoes, rodadas e jogadas.
/// </summary>
public interface IPartidaService
{
    /// <summary>
    /// Cria uma partida aguardando execucao com os jogadores informados.
    /// A ordem dos identificadores representa o assento na mesa e define a composicao dos times.
    /// </summary>
    /// <param name="jogadoresIds">Os identificadores dos jogadores participantes.</param>
    /// <param name="pontuacaoAlvo">A pontuacao que encerra a partida.</param>
    /// <returns>A partida criada.</returns>
    Task<Partida> CriarAsync(IReadOnlyList<Guid> jogadoresIds, int pontuacaoAlvo = Jogo.PontuacaoAlvoPadrao);

    /// <summary>
    /// Executa a partida informada rodada a rodada, persistindo cada rodada e cada jogada.
    /// </summary>
    /// <param name="partidaId">O identificador da partida.</param>
    /// <returns>O resultado consolidado da partida.</returns>
    Task<ResumoPartida> ExecutarAsync(Guid partidaId);

    /// <summary>
    /// Cria e executa uma partida em uma unica operacao.
    /// </summary>
    /// <param name="jogadoresIds">Os identificadores dos jogadores participantes.</param>
    /// <param name="pontuacaoAlvo">A pontuacao que encerra a partida.</param>
    /// <returns>O resultado consolidado da partida.</returns>
    Task<ResumoPartida> CriarEExecutarAsync(IReadOnlyList<Guid> jogadoresIds, int pontuacaoAlvo = Jogo.PontuacaoAlvoPadrao);

    /// <summary>
    /// Obtem a partida informada com times, participacoes, rodadas e jogadas carregados.
    /// </summary>
    /// <param name="partidaId">O identificador da partida.</param>
    /// <returns>A partida encontrada.</returns>
    Task<Partida> ObterAsync(Guid partidaId);

    /// <summary>
    /// Lista as ultimas partidas registradas.
    /// </summary>
    /// <param name="quantidade">A quantidade maxima de partidas retornadas.</param>
    /// <returns>As partidas mais recentes.</returns>
    Task<IReadOnlyList<Partida>> ListarUltimasAsync(int quantidade);

    /// <summary>
    /// Lista as partidas disputadas pelo jogador informado.
    /// </summary>
    /// <param name="jogadorId">O identificador do jogador.</param>
    /// <returns>As partidas do jogador.</returns>
    Task<IReadOnlyList<Partida>> ListarPorJogadorAsync(Guid jogadorId);

    /// <summary>
    /// Cancela uma partida que ainda nao foi finalizada.
    /// </summary>
    /// <param name="partidaId">O identificador da partida.</param>
    Task CancelarAsync(Guid partidaId);
}
