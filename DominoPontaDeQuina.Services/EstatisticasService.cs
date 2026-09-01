using DominoPontaDeQuina.Domain.Entities;
using DominoPontaDeQuina.Repository.Interfaces;
using DominoPontaDeQuina.Services.Exceptions;
using DominoPontaDeQuina.Services.Interfaces;
using DominoPontaDeQuina.Services.Models;

namespace DominoPontaDeQuina.Services;

/// <inheritdoc cref="IEstatisticasService"/>
public class EstatisticasService : IEstatisticasService
{
    /// <summary>
    /// Quantidade padrao de jogadores apresentada no ranking.
    /// </summary>
    private const int TamanhoPadraoDoRanking = 10;

    private readonly IJogadorRepository _jogadorRepository;
    private readonly IParticipacaoPartidaRepository _participacaoRepository;
    private readonly IRodadaRepository _rodadaRepository;
    private readonly IJogadaRepository _jogadaRepository;

    public EstatisticasService(
        IJogadorRepository jogadorRepository,
        IParticipacaoPartidaRepository participacaoRepository,
        IRodadaRepository rodadaRepository,
        IJogadaRepository jogadaRepository)
    {
        _jogadorRepository = jogadorRepository;
        _participacaoRepository = participacaoRepository;
        _rodadaRepository = rodadaRepository;
        _jogadaRepository = jogadaRepository;
    }

    /// <inheritdoc />
    /// <exception cref="RecursoNaoEncontradoException">Quando o jogador nao existir.</exception>
    public async Task<EstatisticasJogador> ObterDoJogadorAsync(Guid jogadorId)
    {
        var jogador = await _jogadorRepository.ObterPorIdAsync(jogadorId)
            ?? throw RecursoNaoEncontradoException.Para("jogador", jogadorId);

        return await MontarEstatisticasAsync(jogador);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<EstatisticasJogador>> ObterRankingAsync(int quantidade = TamanhoPadraoDoRanking)
    {
        var jogadores = await _jogadorRepository.ListarTodosAsync();
        var ranking = new List<EstatisticasJogador>(jogadores.Count);

        foreach (var jogador in jogadores)
            ranking.Add(await MontarEstatisticasAsync(jogador));

        return ranking
            .OrderByDescending(estatisticas => estatisticas.PartidasVencidas)
            .ThenByDescending(estatisticas => estatisticas.PontuacaoTotal)
            .ThenBy(estatisticas => estatisticas.NomeExibicao)
            .Take(quantidade < 1 ? TamanhoPadraoDoRanking : quantidade)
            .ToList();
    }

    /// <summary>
    /// Monta o desempenho acumulado do jogador a partir das consultas dos repositorios.
    /// </summary>
    /// <param name="jogador">O jogador consultado.</param>
    /// <returns>As estatisticas do jogador.</returns>
    private async Task<EstatisticasJogador> MontarEstatisticasAsync(Jogador jogador)
    {
        return new EstatisticasJogador(
            jogador.Id,
            jogador.NomeExibicao,
            await _participacaoRepository.ContarPartidasDoJogadorAsync(jogador.Id),
            await _participacaoRepository.ContarVitoriasDoJogadorAsync(jogador.Id),
            await _rodadaRepository.ContarVitoriasDoJogadorAsync(jogador.Id),
            await _jogadaRepository.ContarPorJogadorAsync(jogador.Id),
            await _jogadaRepository.ContarPassesDoJogadorAsync(jogador.Id),
            await _participacaoRepository.SomarPontuacaoDoJogadorAsync(jogador.Id));
    }
}
