using DominoPontaDeQuina.Domain.Enums;
using DominoPontaDeQuina.Repository.Interfaces;
using DominoPontaDeQuina.Services.Exceptions;
using DominoPontaDeQuina.Services.Interfaces;
using DominoPontaDeQuina.Tests.Infraestrutura;
using Microsoft.Extensions.DependencyInjection;

namespace DominoPontaDeQuina.Tests.Servicos;

/// <summary>
/// Testes do fluxo principal da partida: montagem da mesa, execução pelo motor do jogo e persistência do resultado.
/// </summary>
public class PartidaServiceTests : IDisposable
{
    /// <summary>
    /// Pontuação alvo reduzida para manter a execução dos testes rápida.
    /// </summary>
    private const int PontuacaoAlvoDoTeste = 15;

    private readonly AmbienteDeTestes _ambiente = new();

    /// <summary>
    /// <b>Objetivo:</b> Garantir que os jogadores sejam distribuídos alternadamente entre os dois times.
    /// <br/><b>Critério:</b> Cada time deve receber dois jogadores e os vizinhos de mesa devem ficar em times opostos.
    /// </summary>
    [Trait("Categoria", "Servico")]
    [Fact(DisplayName = "Deve montar dois times distribuindo os assentos da mesa alternadamente.")]
    public async Task CriarAsync_DeveMontarDoisTimesComAssentosAlternados()
    {
        var (_, jogadores) = await _ambiente.CriarMesaAsync("Ana", "Bruno", "Carla", "Diego");
        var ids = jogadores.Select(jogador => jogador.Id).ToList();

        using var escopo = _ambiente.CriarEscopo();
        var partidaService = escopo.ServiceProvider.GetRequiredService<IPartidaService>();

        var criada = await partidaService.CriarAsync(ids, PontuacaoAlvoDoTeste);
        var partida = await partidaService.ObterAsync(criada.Id);

        Assert.Equal(StatusPartida.Aguardando, partida.Status);
        Assert.Equal(2, partida.Times.Count);
        Assert.All(partida.Times, time => Assert.Equal(2, partida.Participacoes.Count(p => p.TimePartidaId == time.Id)));

        var timeDeAna = partida.Participacoes.First(p => p.JogadorId == ids[0]).TimePartidaId;
        var timeDeCarla = partida.Participacoes.First(p => p.JogadorId == ids[2]).TimePartidaId;
        var timeDeBruno = partida.Participacoes.First(p => p.JogadorId == ids[1]).TimePartidaId;

        Assert.Equal(timeDeAna, timeDeCarla);
        Assert.NotEqual(timeDeAna, timeDeBruno);
    }

    /// <summary>
    /// <b>Objetivo:</b> Garantir que a partida completa seja executada e persistida em todos os níveis do modelo.
    /// <br/><b>Critério:</b> A partida deve terminar finalizada, com rodadas, jogadas e um único time vencedor gravados.
    /// </summary>
    [Trait("Categoria", "FluxoPrincipal")]
    [Fact(DisplayName = "Deve executar a partida completa e persistir partida, rodadas e jogadas.")]
    public async Task CriarEExecutarAsync_DevePersistirTodoOFluxoDaPartida()
    {
        var (_, jogadores) = await _ambiente.CriarMesaAsync("Ana", "Bruno", "Carla", "Diego");
        var ids = jogadores.Select(jogador => jogador.Id).ToList();

        var resumo = await _ambiente.UsarAsync<IPartidaService, Services.Models.ResumoPartida>(
            servico => servico.CriarEExecutarAsync(ids, PontuacaoAlvoDoTeste));

        Assert.True(resumo.TotalDeRodadas > 0);
        Assert.True(resumo.TotalDeJogadas > 0);
        Assert.NotEmpty(resumo.TimeVencedor);
        Assert.Single(resumo.Placar, time => time.Vencedor);

        using var escopo = _ambiente.CriarEscopo();
        var partidaService = escopo.ServiceProvider.GetRequiredService<IPartidaService>();
        var rodadaRepository = escopo.ServiceProvider.GetRequiredService<IRodadaRepository>();
        var jogadaRepository = escopo.ServiceProvider.GetRequiredService<IJogadaRepository>();

        var partida = await partidaService.ObterAsync(resumo.PartidaId);
        var rodadas = await rodadaRepository.ListarPorPartidaAsync(partida.Id);

        Assert.Equal(StatusPartida.Finalizada, partida.Status);
        Assert.NotNull(partida.FinalizadoEm);
        Assert.Equal(resumo.TotalDeRodadas, rodadas.Count);
        Assert.Equal(Enumerable.Range(1, rodadas.Count), rodadas.Select(rodada => rodada.Numero));
        Assert.All(rodadas, rodada => Assert.Equal(StatusRodada.Finalizada, rodada.Status));
        Assert.All(rodadas, rodada => Assert.NotNull(rodada.TipoFinalizacao));

        var totalDeJogadasPersistidas = 0;

        foreach (var rodada in rodadas)
        {
            var jogadas = await jogadaRepository.ListarPorRodadaAsync(rodada.Id);

            Assert.NotEmpty(jogadas);
            Assert.All(jogadas, jogada => Assert.Contains(jogada.JogadorId, ids));

            totalDeJogadasPersistidas += jogadas.Count;
        }

        Assert.Equal(resumo.TotalDeJogadas, totalDeJogadasPersistidas);
        Assert.Equal(2, partida.Participacoes.Count(participacao => participacao.Vencedor));
        Assert.Single(partida.Times, time => time.Vencedor);
        Assert.True(partida.Times.Single(time => time.Vencedor).Pontuacao >= PontuacaoAlvoDoTeste);
    }

    /// <summary>
    /// <b>Objetivo:</b> Garantir que a pontuação persistida no jogador reflita o que foi apurado nas jogadas.
    /// <br/><b>Critério:</b> A soma das participações deve ser igual à soma dos pontos gerados pelas jogadas.
    /// </summary>
    [Trait("Categoria", "FluxoPrincipal")]
    [Fact(DisplayName = "Deve manter a pontuação das participações coerente com os pontos gerados nas jogadas.")]
    public async Task CriarEExecutarAsync_DeveManterPontuacaoCoerenteComAsJogadas()
    {
        var (_, jogadores) = await _ambiente.CriarMesaAsync("Ana", "Bruno", "Carla", "Diego");
        var ids = jogadores.Select(jogador => jogador.Id).ToList();

        var resumo = await _ambiente.UsarAsync<IPartidaService, Services.Models.ResumoPartida>(
            servico => servico.CriarEExecutarAsync(ids, PontuacaoAlvoDoTeste));

        using var escopo = _ambiente.CriarEscopo();
        var partidaService = escopo.ServiceProvider.GetRequiredService<IPartidaService>();
        var jogadaRepository = escopo.ServiceProvider.GetRequiredService<IJogadaRepository>();

        var partida = await partidaService.ObterAsync(resumo.PartidaId);

        foreach (var participacao in partida.Participacoes)
        {
            var pontosDasJogadas = await jogadaRepository.SomarPontosDoJogadorAsync(participacao.JogadorId);

            Assert.Equal(pontosDasJogadas, participacao.Pontuacao);
        }

        foreach (var time in partida.Times)
        {
            var pontosDoTime = partida.Participacoes
                .Where(participacao => participacao.TimePartidaId == time.Id)
                .Sum(participacao => participacao.Pontuacao);

            Assert.Equal(pontosDoTime, time.Pontuacao);
        }
    }

    /// <summary>
    /// <b>Objetivo:</b> Garantir que a mesa exija uma quantidade par de jogadores.
    /// <br/><b>Critério:</b> Deve lançar <see cref="RegraDeNegocioException"/> para três jogadores.
    /// </summary>
    [Trait("Categoria", "Excecao")]
    [Fact(DisplayName = "Deve lançar exceção de regra de negócio ao montar mesa com quantidade ímpar de jogadores.")]
    public async Task CriarAsync_DeveLancarExcecao_QuandoQuantidadeDeJogadoresForImpar()
    {
        var (_, jogadores) = await _ambiente.CriarMesaAsync("Ana", "Bruno", "Carla");
        var ids = jogadores.Select(jogador => jogador.Id).ToList();

        await Assert.ThrowsAsync<RegraDeNegocioException>(() =>
            _ambiente.UsarAsync<IPartidaService, Domain.Entities.Partida>(
                servico => servico.CriarAsync(ids, PontuacaoAlvoDoTeste)));
    }

    /// <summary>
    /// <b>Objetivo:</b> Garantir que o mesmo jogador não ocupe dois assentos da mesa.
    /// <br/><b>Critério:</b> Deve lançar <see cref="RegraDeNegocioException"/> quando houver identificador repetido.
    /// </summary>
    [Trait("Categoria", "Excecao")]
    [Fact(DisplayName = "Deve lançar exceção de regra de negócio ao repetir um jogador na mesma mesa.")]
    public async Task CriarAsync_DeveLancarExcecao_QuandoJogadorForRepetido()
    {
        var (_, jogadores) = await _ambiente.CriarMesaAsync("Ana", "Bruno");
        var ids = new List<Guid> { jogadores[0].Id, jogadores[0].Id };

        await Assert.ThrowsAsync<RegraDeNegocioException>(() =>
            _ambiente.UsarAsync<IPartidaService, Domain.Entities.Partida>(
                servico => servico.CriarAsync(ids, PontuacaoAlvoDoTeste)));
    }

    /// <summary>
    /// <b>Objetivo:</b> Garantir que jogadores inexistentes não sejam aceitos na mesa.
    /// <br/><b>Critério:</b> Deve lançar <see cref="RecursoNaoEncontradoException"/>.
    /// </summary>
    [Trait("Categoria", "Excecao")]
    [Fact(DisplayName = "Deve lançar exceção de recurso não encontrado ao montar mesa com jogador inexistente.")]
    public async Task CriarAsync_DeveLancarExcecao_QuandoJogadorNaoExistir()
    {
        var (_, jogadores) = await _ambiente.CriarMesaAsync("Ana", "Bruno");
        var ids = new List<Guid> { jogadores[0].Id, Guid.NewGuid() };

        await Assert.ThrowsAsync<RecursoNaoEncontradoException>(() =>
            _ambiente.UsarAsync<IPartidaService, Domain.Entities.Partida>(
                servico => servico.CriarAsync(ids, PontuacaoAlvoDoTeste)));
    }

    /// <summary>
    /// <b>Objetivo:</b> Garantir que uma partida já finalizada não seja executada novamente.
    /// <br/><b>Critério:</b> Deve lançar <see cref="RegraDeNegocioException"/> na segunda execução.
    /// </summary>
    [Trait("Categoria", "Excecao")]
    [Fact(DisplayName = "Deve lançar exceção de regra de negócio ao executar uma partida já finalizada.")]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoPartidaJaFoiFinalizada()
    {
        var (_, jogadores) = await _ambiente.CriarMesaAsync("Ana", "Bruno");
        var ids = jogadores.Select(jogador => jogador.Id).ToList();

        var resumo = await _ambiente.UsarAsync<IPartidaService, Services.Models.ResumoPartida>(
            servico => servico.CriarEExecutarAsync(ids, PontuacaoAlvoDoTeste));

        await Assert.ThrowsAsync<RegraDeNegocioException>(() =>
            _ambiente.UsarAsync<IPartidaService, Services.Models.ResumoPartida>(
                servico => servico.ExecutarAsync(resumo.PartidaId)));
    }

    /// <summary>
    /// <b>Objetivo:</b> Garantir o cancelamento de uma partida que ainda não foi disputada.
    /// <br/><b>Critério:</b> O status persistido deve ser <see cref="StatusPartida.Cancelada"/>.
    /// </summary>
    [Trait("Categoria", "Servico")]
    [Fact(DisplayName = "Deve cancelar a partida que ainda não foi finalizada.")]
    public async Task CancelarAsync_DeveMarcarPartidaComoCancelada()
    {
        var (_, jogadores) = await _ambiente.CriarMesaAsync("Ana", "Bruno");
        var ids = jogadores.Select(jogador => jogador.Id).ToList();

        using var escopo = _ambiente.CriarEscopo();
        var partidaService = escopo.ServiceProvider.GetRequiredService<IPartidaService>();

        var partida = await partidaService.CriarAsync(ids, PontuacaoAlvoDoTeste);

        await partidaService.CancelarAsync(partida.Id);

        var cancelada = await partidaService.ObterAsync(partida.Id);

        Assert.Equal(StatusPartida.Cancelada, cancelada.Status);
        Assert.NotNull(cancelada.FinalizadoEm);
    }

    /// <summary>
    /// <b>Objetivo:</b> Garantir que partidas inexistentes sejam sinalizadas ao chamador.
    /// <br/><b>Critério:</b> Deve lançar <see cref="RecursoNaoEncontradoException"/>.
    /// </summary>
    [Trait("Categoria", "Excecao")]
    [Fact(DisplayName = "Deve lançar exceção de recurso não encontrado ao consultar partida inexistente.")]
    public async Task ObterAsync_DeveLancarExcecao_QuandoPartidaNaoExistir()
    {
        await Assert.ThrowsAsync<RecursoNaoEncontradoException>(() =>
            _ambiente.UsarAsync<IPartidaService, Domain.Entities.Partida>(
                servico => servico.ObterAsync(Guid.NewGuid())));
    }

    /// <inheritdoc />
    public void Dispose() => _ambiente.Dispose();
}
