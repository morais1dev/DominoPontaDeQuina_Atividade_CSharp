using DominoPontaDeQuina.Services.Exceptions;
using DominoPontaDeQuina.Services.Interfaces;
using DominoPontaDeQuina.Services.Models;
using DominoPontaDeQuina.Tests.Infraestrutura;
using Microsoft.Extensions.DependencyInjection;

namespace DominoPontaDeQuina.Tests.Servicos;

/// <summary>
/// Testes das consultas de desempenho montadas a partir das contagens dos repositórios.
/// </summary>
public class EstatisticasServiceTests : IDisposable
{
    /// <summary>
    /// Pontuação alvo reduzida para manter a execução dos testes rápida.
    /// </summary>
    private const int PontuacaoAlvoDoTeste = 15;

    private readonly AmbienteDeTestes _ambiente = new();

    /// <summary>
    /// <b>Objetivo:</b> Garantir que o desempenho do jogador reflita a partida disputada.
    /// <br/><b>Critério:</b> Partidas disputadas, jogadas registradas e pontuação devem ser consistentes.
    /// </summary>
    [Trait("Categoria", "Servico")]
    [Fact(DisplayName = "Deve consolidar o desempenho do jogador após a partida ser disputada.")]
    public async Task ObterDoJogadorAsync_DeveConsolidarDesempenhoDoJogador()
    {
        var (_, jogadores) = await _ambiente.CriarMesaAsync("Ana", "Bruno", "Carla", "Diego");
        var ids = jogadores.Select(jogador => jogador.Id).ToList();

        await _ambiente.UsarAsync<IPartidaService, ResumoPartida>(
            servico => servico.CriarEExecutarAsync(ids, PontuacaoAlvoDoTeste));

        var estatisticas = await _ambiente.UsarAsync<IEstatisticasService, EstatisticasJogador>(
            servico => servico.ObterDoJogadorAsync(ids[0]));

        Assert.Equal(ids[0], estatisticas.JogadorId);
        Assert.Equal("Ana", estatisticas.NomeExibicao);
        Assert.Equal(1, estatisticas.PartidasDisputadas);
        Assert.InRange(estatisticas.PartidasVencidas, 0, 1);
        Assert.True(estatisticas.JogadasRealizadas > 0);
        Assert.True(estatisticas.VezesQuePassou <= estatisticas.JogadasRealizadas);
        Assert.True(estatisticas.PontuacaoTotal >= 0);
    }

    /// <summary>
    /// <b>Objetivo:</b> Garantir a ordenação do ranking por vitórias e, no empate, por pontuação.
    /// <br/><b>Critério:</b> A lista deve estar ordenada de forma decrescente pelos dois critérios.
    /// </summary>
    [Trait("Categoria", "Servico")]
    [Fact(DisplayName = "Deve ordenar o ranking por vitórias e por pontuação total.")]
    public async Task ObterRankingAsync_DeveOrdenarPorVitoriasEPontuacao()
    {
        var (_, jogadores) = await _ambiente.CriarMesaAsync("Ana", "Bruno", "Carla", "Diego");
        var ids = jogadores.Select(jogador => jogador.Id).ToList();

        await _ambiente.UsarAsync<IPartidaService, ResumoPartida>(
            servico => servico.CriarEExecutarAsync(ids, PontuacaoAlvoDoTeste));

        var ranking = await _ambiente.UsarAsync<IEstatisticasService, IReadOnlyList<EstatisticasJogador>>(
            servico => servico.ObterRankingAsync());

        Assert.Equal(jogadores.Count, ranking.Count);
        Assert.Equal(2, ranking.Count(estatisticas => estatisticas.PartidasVencidas == 1));

        var ordenado = ranking
            .OrderByDescending(estatisticas => estatisticas.PartidasVencidas)
            .ThenByDescending(estatisticas => estatisticas.PontuacaoTotal)
            .ThenBy(estatisticas => estatisticas.NomeExibicao)
            .ToList();

        Assert.Equal(ordenado.Select(estatisticas => estatisticas.JogadorId), ranking.Select(estatisticas => estatisticas.JogadorId));
    }

    /// <summary>
    /// <b>Objetivo:</b> Garantir que o ranking respeite o limite de jogadores solicitado.
    /// <br/><b>Critério:</b> A quantidade retornada não deve ultrapassar o limite informado.
    /// </summary>
    [Trait("Categoria", "Servico")]
    [Fact(DisplayName = "Deve limitar a quantidade de jogadores retornados no ranking.")]
    public async Task ObterRankingAsync_DeveRespeitarOLimiteInformado()
    {
        await _ambiente.CriarMesaAsync("Ana", "Bruno", "Carla", "Diego");

        var ranking = await _ambiente.UsarAsync<IEstatisticasService, IReadOnlyList<EstatisticasJogador>>(
            servico => servico.ObterRankingAsync(2));

        Assert.Equal(2, ranking.Count);
    }

    /// <summary>
    /// <b>Objetivo:</b> Garantir que jogadores inexistentes sejam sinalizados ao chamador.
    /// <br/><b>Critério:</b> Deve lançar <see cref="RecursoNaoEncontradoException"/>.
    /// </summary>
    [Trait("Categoria", "Excecao")]
    [Fact(DisplayName = "Deve lançar exceção de recurso não encontrado ao consultar estatísticas de jogador inexistente.")]
    public async Task ObterDoJogadorAsync_DeveLancarExcecao_QuandoJogadorNaoExistir()
    {
        await Assert.ThrowsAsync<RecursoNaoEncontradoException>(() =>
            _ambiente.UsarAsync<IEstatisticasService, EstatisticasJogador>(
                servico => servico.ObterDoJogadorAsync(Guid.NewGuid())));
    }

    /// <inheritdoc />
    public void Dispose() => _ambiente.Dispose();
}
