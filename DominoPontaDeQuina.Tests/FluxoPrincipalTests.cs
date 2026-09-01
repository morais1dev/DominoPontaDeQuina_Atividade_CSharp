using DominoPontaDeQuina.App;
using DominoPontaDeQuina.Domain.Enums;
using DominoPontaDeQuina.Repository.Interfaces;
using DominoPontaDeQuina.Tests.Infraestrutura;
using Microsoft.Extensions.DependencyInjection;

namespace DominoPontaDeQuina.Tests;

/// <summary>
/// Testes do fluxo principal da aplicação de console, resolvido pelo mesmo container configurado em <c>Program.cs</c>.
/// </summary>
public class FluxoPrincipalTests : IDisposable
{
    /// <summary>
    /// Pontuação alvo reduzida para manter a execução dos testes rápida.
    /// </summary>
    private const int PontuacaoAlvoDoTeste = 15;

    private readonly AmbienteDeTestes _ambiente = new();

    /// <summary>
    /// <b>Objetivo:</b> Garantir que a classe de entrada receba suas dependências por construtor e conclua o fluxo.
    /// <br/><b>Critério:</b> A execução deve retornar código de saída zero e persistir uma partida finalizada.
    /// </summary>
    [Trait("Categoria", "FluxoPrincipal")]
    [Fact(DisplayName = "Deve executar o fluxo principal do console e persistir a partida disputada.")]
    public async Task ExecutarAsync_DeveConcluirFluxoPrincipalEPersistirPartida()
    {
        using var escopo = _ambiente.CriarEscopo();
        var aplicacao = escopo.ServiceProvider.GetRequiredService<AplicacaoConsole>();
        var partidaRepository = escopo.ServiceProvider.GetRequiredService<IPartidaRepository>();
        var usuarioRepository = escopo.ServiceProvider.GetRequiredService<IUsuarioRepository>();

        var codigoDeSaida = await ExecutarCapturandoSaidaAsync(aplicacao, PontuacaoAlvoDoTeste);

        Assert.Equal(0, codigoDeSaida);

        var finalizadas = await partidaRepository.ListarPorStatusAsync(StatusPartida.Finalizada);

        Assert.Single(finalizadas);
        Assert.NotNull(await usuarioRepository.ObterPorEmailAsync("torneio@domino.local"));
    }

    /// <summary>
    /// <b>Objetivo:</b> Garantir que a segunda execução reaproveite a conta e os jogadores já cadastrados.
    /// <br/><b>Critério:</b> Devem existir duas partidas finalizadas e continuar existindo apenas quatro jogadores.
    /// </summary>
    [Trait("Categoria", "FluxoPrincipal")]
    [Fact(DisplayName = "Deve reaproveitar a conta e os jogadores já cadastrados em uma nova execução.")]
    public async Task ExecutarAsync_DeveReaproveitarContaEJogadoresEmNovaExecucao()
    {
        await ExecutarEmEscopoAsync();
        await ExecutarEmEscopoAsync();

        using var escopo = _ambiente.CriarEscopo();
        var partidaRepository = escopo.ServiceProvider.GetRequiredService<IPartidaRepository>();
        var jogadorRepository = escopo.ServiceProvider.GetRequiredService<IJogadorRepository>();

        var finalizadas = await partidaRepository.ListarPorStatusAsync(StatusPartida.Finalizada);
        var jogadores = await jogadorRepository.ListarTodosAsync();

        Assert.Equal(2, finalizadas.Count);
        Assert.Equal(4, jogadores.Count);
    }

    /// <summary>
    /// Executa a aplicação de console em um escopo próprio, como faz o <c>Program.cs</c>.
    /// </summary>
    private async Task ExecutarEmEscopoAsync()
    {
        using var escopo = _ambiente.CriarEscopo();
        var aplicacao = escopo.ServiceProvider.GetRequiredService<AplicacaoConsole>();

        Assert.Equal(0, await ExecutarCapturandoSaidaAsync(aplicacao, PontuacaoAlvoDoTeste));
    }

    /// <summary>
    /// Executa a aplicação redirecionando a saída padrão para não poluir o relatório dos testes.
    /// </summary>
    /// <param name="aplicacao">A aplicação de console resolvida pelo container.</param>
    /// <param name="pontuacaoAlvo">A pontuação que encerra a partida.</param>
    /// <returns>O código de saída retornado pela aplicação.</returns>
    private static async Task<int> ExecutarCapturandoSaidaAsync(AplicacaoConsole aplicacao, int pontuacaoAlvo)
    {
        var saidaOriginal = Console.Out;

        try
        {
            Console.SetOut(TextWriter.Null);

            return await aplicacao.ExecutarAsync(pontuacaoAlvo);
        }
        finally
        {
            Console.SetOut(saidaOriginal);
        }
    }

    /// <inheritdoc />
    public void Dispose() => _ambiente.Dispose();
}
