using DominoPontaDeQuina.Domain.Entities;
using DominoPontaDeQuina.Domain.Enums;

namespace DominoPontaDeQuina.Repository.Interfaces;

/// <summary>
/// Define as consultas de persistencia disponiveis para as rodadas de uma partida.
/// </summary>
public interface IRodadaRepository : IRepositorioBase<Rodada>
{
    /// <summary>
    /// Lista as rodadas da partida informada em ordem crescente de numero.
    /// </summary>
    /// <param name="partidaId">O identificador da partida.</param>
    /// <returns>As rodadas da partida.</returns>
    Task<List<Rodada>> ListarPorPartidaAsync(Guid partidaId);

    /// <summary>
    /// Obtem a rodada mais recente da partida informada.
    /// </summary>
    /// <param name="partidaId">O identificador da partida.</param>
    /// <returns>A ultima rodada registrada, ou <see langword="null"/> quando a partida ainda nao tiver rodadas.</returns>
    Task<Rodada?> ObterUltimaDaPartidaAsync(Guid partidaId);

    /// <summary>
    /// Conta quantas rodadas ja foram registradas para a partida informada.
    /// </summary>
    /// <param name="partidaId">O identificador da partida.</param>
    /// <returns>A quantidade de rodadas da partida.</returns>
    Task<int> ContarPorPartidaAsync(Guid partidaId);

    /// <summary>
    /// Conta quantas rodadas o jogador informado venceu.
    /// </summary>
    /// <param name="jogadorId">O identificador do jogador.</param>
    /// <returns>A quantidade de rodadas vencidas.</returns>
    Task<int> ContarVitoriasDoJogadorAsync(Guid jogadorId);

    /// <summary>
    /// Lista as rodadas encerradas pelo tipo de finalizacao informado.
    /// </summary>
    /// <param name="tipoFinalizacao">O motivo de encerramento pesquisado.</param>
    /// <returns>As rodadas encontradas, da mais recente para a mais antiga.</returns>
    Task<List<Rodada>> ListarPorTipoFinalizacaoAsync(TipoFinalizacaoRodada tipoFinalizacao);
}
