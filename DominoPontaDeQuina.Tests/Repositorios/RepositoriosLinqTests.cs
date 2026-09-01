using DominoPontaDeQuina.Domain.Enums;
using DominoPontaDeQuina.Repository.Interfaces;
using DominoPontaDeQuina.Services.Interfaces;
using DominoPontaDeQuina.Services.Models;
using DominoPontaDeQuina.Tests.Infraestrutura;
using Microsoft.Extensions.DependencyInjection;

namespace DominoPontaDeQuina.Tests.Repositorios;

/// <summary>
/// Testes das consultas LINQ que permanecem concentradas na camada de repositório,
/// executadas contra um banco SQLite em memória.
/// </summary>
public class RepositoriosLinqTests : IDisposable
{
    /// <summary>
    /// Pontuação alvo reduzida para manter a execução dos testes rápida.
    /// </summary>
    private const int PontuacaoAlvoDoTeste = 15;

    private readonly AmbienteDeTestes _ambiente = new();

    /// <summary>
    /// <b>Objetivo:</b> Garantir a consulta de existência de e-mail usada no cadastro.
    /// <br/><b>Critério:</b> Deve encontrar o e-mail cadastrado e ignorar um e-mail desconhecido.
    /// </summary>
    [Trait("Categoria", "Repositorio")]
    [Fact(DisplayName = "Deve identificar se o e-mail já está cadastrado na consulta do repositório de usuários.")]
    public async Task UsuarioRepository_DeveIdentificarEmailJaCadastrado()
    {
        await _ambiente.CriarMesaAsync("Ana");

        using var escopo = _ambiente.CriarEscopo();
        var repositorio = escopo.ServiceProvider.GetRequiredService<IUsuarioRepository>();

        Assert.True(await repositorio.EmailJaCadastradoAsync("mesa@domino.local"));
        Assert.False(await repositorio.EmailJaCadastradoAsync("outro@domino.local"));
    }

    /// <summary>
    /// <b>Objetivo:</b> Garantir o filtro por trecho de nome no repositório de jogadores.
    /// <br/><b>Critério:</b> Apenas os jogadores que contêm o trecho devem ser retornados.
    /// </summary>
    [Trait("Categoria", "Repositorio")]
    [Fact(DisplayName = "Deve filtrar jogadores pelo trecho do nome de exibição.")]
    public async Task JogadorRepository_DeveFiltrarPorTrechoDoNome()
    {
        await _ambiente.CriarMesaAsync("Ana", "Bruno", "Carla", "Diego");

        using var escopo = _ambiente.CriarEscopo();
        var repositorio = escopo.ServiceProvider.GetRequiredService<IJogadorRepository>();

        var encontrados = await repositorio.BuscarPorNomeExibicaoAsync("ar");

        Assert.Single(encontrados);
        Assert.Equal("Carla", encontrados[0].NomeExibicao);
    }

    /// <summary>
    /// <b>Objetivo:</b> Garantir a consulta por identificadores usada na montagem da mesa.
    /// <br/><b>Critério:</b> Somente os jogadores informados devem ser retornados.
    /// </summary>
    [Trait("Categoria", "Repositorio")]
    [Fact(DisplayName = "Deve listar apenas os jogadores cujos identificadores foram informados.")]
    public async Task JogadorRepository_DeveListarPorIdentificadores()
    {
        var (_, jogadores) = await _ambiente.CriarMesaAsync("Ana", "Bruno", "Carla", "Diego");
        var ids = new[] { jogadores[0].Id, jogadores[3].Id };

        using var escopo = _ambiente.CriarEscopo();
        var repositorio = escopo.ServiceProvider.GetRequiredService<IJogadorRepository>();

        var encontrados = await repositorio.ListarPorIdsAsync(ids);

        Assert.Equal(2, encontrados.Count);
        Assert.All(encontrados, jogador => Assert.Contains(jogador.Id, ids));
    }

    /// <summary>
    /// <b>Objetivo:</b> Garantir o filtro por status no repositório de partidas.
    /// <br/><b>Critério:</b> A partida disputada deve aparecer apenas na consulta por finalizada.
    /// </summary>
    [Trait("Categoria", "Repositorio")]
    [Fact(DisplayName = "Deve filtrar partidas pelo status persistido.")]
    public async Task PartidaRepository_DeveFiltrarPorStatus()
    {
        var partidaId = await DisputarPartidaAsync();

        using var escopo = _ambiente.CriarEscopo();
        var repositorio = escopo.ServiceProvider.GetRequiredService<IPartidaRepository>();

        var finalizadas = await repositorio.ListarPorStatusAsync(StatusPartida.Finalizada);
        var aguardando = await repositorio.ListarPorStatusAsync(StatusPartida.Aguardando);

        Assert.Single(finalizadas, partida => partida.Id == partidaId);
        Assert.Empty(aguardando);
    }

    /// <summary>
    /// <b>Objetivo:</b> Garantir a consulta de partidas por jogador participante.
    /// <br/><b>Critério:</b> A partida deve aparecer para quem jogou e não aparecer para um jogador aleatório.
    /// </summary>
    [Trait("Categoria", "Repositorio")]
    [Fact(DisplayName = "Deve listar as partidas em que o jogador participou.")]
    public async Task PartidaRepository_DeveListarPartidasDoJogador()
    {
        var (_, jogadores) = await _ambiente.CriarMesaAsync("Ana", "Bruno", "Carla", "Diego");
        var ids = jogadores.Select(jogador => jogador.Id).ToList();

        var resumo = await _ambiente.UsarAsync<IPartidaService, ResumoPartida>(
            servico => servico.CriarEExecutarAsync(ids, PontuacaoAlvoDoTeste));

        using var escopo = _ambiente.CriarEscopo();
        var repositorio = escopo.ServiceProvider.GetRequiredService<IPartidaRepository>();

        var doJogador = await repositorio.ListarPorJogadorAsync(ids[0]);
        var deOutroJogador = await repositorio.ListarPorJogadorAsync(Guid.NewGuid());

        Assert.Single(doJogador, partida => partida.Id == resumo.PartidaId);
        Assert.Empty(deOutroJogador);
    }

    /// <summary>
    /// <b>Objetivo:</b> Garantir a ordenação das rodadas pela consulta do repositório.
    /// <br/><b>Critério:</b> As rodadas devem vir em ordem crescente de número e a última deve ser a de maior número.
    /// </summary>
    [Trait("Categoria", "Repositorio")]
    [Fact(DisplayName = "Deve listar as rodadas da partida em ordem crescente de número.")]
    public async Task RodadaRepository_DeveListarRodadasEmOrdem()
    {
        var partidaId = await DisputarPartidaAsync();

        using var escopo = _ambiente.CriarEscopo();
        var repositorio = escopo.ServiceProvider.GetRequiredService<IRodadaRepository>();

        var rodadas = await repositorio.ListarPorPartidaAsync(partidaId);
        var ultima = await repositorio.ObterUltimaDaPartidaAsync(partidaId);
        var total = await repositorio.ContarPorPartidaAsync(partidaId);

        Assert.NotEmpty(rodadas);
        Assert.Equal(rodadas.OrderBy(rodada => rodada.Numero).Select(rodada => rodada.Id), rodadas.Select(rodada => rodada.Id));
        Assert.Equal(rodadas.Count, total);
        Assert.Equal(rodadas[^1].Id, ultima?.Id);
    }

    /// <summary>
    /// <b>Objetivo:</b> Garantir as contagens de jogadas mantidas no repositório.
    /// <br/><b>Critério:</b> As passagens de vez devem ser um subconjunto das jogadas do jogador.
    /// </summary>
    [Trait("Categoria", "Repositorio")]
    [Fact(DisplayName = "Deve contar as jogadas e as passagens de vez de cada jogador.")]
    public async Task JogadaRepository_DeveContarJogadasEPassagens()
    {
        var (_, jogadores) = await _ambiente.CriarMesaAsync("Ana", "Bruno", "Carla", "Diego");
        var ids = jogadores.Select(jogador => jogador.Id).ToList();

        await _ambiente.UsarAsync<IPartidaService, ResumoPartida>(
            servico => servico.CriarEExecutarAsync(ids, PontuacaoAlvoDoTeste));

        using var escopo = _ambiente.CriarEscopo();
        var repositorio = escopo.ServiceProvider.GetRequiredService<IJogadaRepository>();

        foreach (var id in ids)
        {
            var jogadas = await repositorio.ContarPorJogadorAsync(id);
            var passes = await repositorio.ContarPassesDoJogadorAsync(id);

            Assert.True(jogadas > 0);
            Assert.True(passes <= jogadas);
        }
    }

    /// <summary>
    /// <b>Objetivo:</b> Garantir as agregações de participação usadas no ranking.
    /// <br/><b>Critério:</b> A contagem de partidas, de vitórias e a soma de pontos devem refletir a partida disputada.
    /// </summary>
    [Trait("Categoria", "Repositorio")]
    [Fact(DisplayName = "Deve agregar partidas, vitórias e pontuação das participações do jogador.")]
    public async Task ParticipacaoPartidaRepository_DeveAgregarDesempenhoDoJogador()
    {
        var (_, jogadores) = await _ambiente.CriarMesaAsync("Ana", "Bruno", "Carla", "Diego");
        var ids = jogadores.Select(jogador => jogador.Id).ToList();

        var resumo = await _ambiente.UsarAsync<IPartidaService, ResumoPartida>(
            servico => servico.CriarEExecutarAsync(ids, PontuacaoAlvoDoTeste));

        using var escopo = _ambiente.CriarEscopo();
        var repositorio = escopo.ServiceProvider.GetRequiredService<IParticipacaoPartidaRepository>();

        var participacoes = await repositorio.ListarPorPartidaAsync(resumo.PartidaId);
        var vencedor = await repositorio.ObterVencedorDaPartidaAsync(resumo.PartidaId);

        Assert.Equal(4, participacoes.Count);
        Assert.Equal(Enumerable.Range(0, 4), participacoes.Select(participacao => participacao.Posicao));
        Assert.NotNull(vencedor);

        foreach (var id in ids)
        {
            Assert.Equal(1, await repositorio.ContarPartidasDoJogadorAsync(id));
            Assert.InRange(await repositorio.ContarVitoriasDoJogadorAsync(id), 0, 1);
            Assert.True(await repositorio.SomarPontuacaoDoJogadorAsync(id) >= 0);
        }
    }

    /// <summary>
    /// Cria e executa uma partida completa para servir de massa de dados às consultas.
    /// </summary>
    /// <returns>O identificador da partida disputada.</returns>
    private async Task<Guid> DisputarPartidaAsync()
    {
        var (_, jogadores) = await _ambiente.CriarMesaAsync("Ana", "Bruno", "Carla", "Diego");
        var ids = jogadores.Select(jogador => jogador.Id).ToList();

        var resumo = await _ambiente.UsarAsync<IPartidaService, ResumoPartida>(
            servico => servico.CriarEExecutarAsync(ids, PontuacaoAlvoDoTeste));

        return resumo.PartidaId;
    }

    /// <inheritdoc />
    public void Dispose() => _ambiente.Dispose();
}
