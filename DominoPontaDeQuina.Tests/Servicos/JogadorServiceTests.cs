using DominoPontaDeQuina.Services;
using DominoPontaDeQuina.Services.Exceptions;
using DominoPontaDeQuina.Services.Interfaces;
using DominoPontaDeQuina.Tests.Infraestrutura;
using Microsoft.Extensions.DependencyInjection;

namespace DominoPontaDeQuina.Tests.Servicos;

/// <summary>
/// Testes das regras de uso dos perfis de jogador vinculados a um usuário.
/// </summary>
public class JogadorServiceTests : IDisposable
{
    private readonly AmbienteDeTestes _ambiente = new();

    /// <summary>
    /// <b>Objetivo:</b> Garantir que o perfil criado fique vinculado ao usuário informado.
    /// <br/><b>Critério:</b> O jogador persistido deve manter o identificador do usuário e o nome normalizado.
    /// </summary>
    [Trait("Categoria", "Servico")]
    [Fact(DisplayName = "Deve criar o perfil de jogador vinculado ao usuário informado.")]
    public async Task CriarAsync_DeveVincularJogadorAoUsuario()
    {
        using var escopo = _ambiente.CriarEscopo();
        var usuarioService = escopo.ServiceProvider.GetRequiredService<IUsuarioService>();
        var jogadorService = escopo.ServiceProvider.GetRequiredService<IJogadorService>();

        var usuario = await usuarioService.CadastrarAsync("Vitor", "vitor@domino.local", "domino123");

        var jogador = await jogadorService.CriarAsync(usuario.Id, "  Ana  ");

        Assert.Equal(usuario.Id, jogador.UsuarioId);
        Assert.Equal("Ana", jogador.NomeExibicao);
        Assert.NotEqual(Guid.Empty, jogador.Id);
    }

    /// <summary>
    /// <b>Objetivo:</b> Garantir que o mesmo usuário não repita nomes de exibição.
    /// <br/><b>Critério:</b> A segunda criação com o mesmo nome deve lançar <see cref="RegraDeNegocioException"/>.
    /// </summary>
    [Trait("Categoria", "Excecao")]
    [Fact(DisplayName = "Deve lançar exceção de regra de negócio ao repetir o nome de exibição no mesmo usuário.")]
    public async Task CriarAsync_DeveLancarExcecao_QuandoNomeJaUtilizado()
    {
        using var escopo = _ambiente.CriarEscopo();
        var usuarioService = escopo.ServiceProvider.GetRequiredService<IUsuarioService>();
        var jogadorService = escopo.ServiceProvider.GetRequiredService<IJogadorService>();

        var usuario = await usuarioService.CadastrarAsync("Vitor", "vitor@domino.local", "domino123");

        await jogadorService.CriarAsync(usuario.Id, "Ana");

        await Assert.ThrowsAsync<RegraDeNegocioException>(() => jogadorService.CriarAsync(usuario.Id, "Ana"));
    }

    /// <summary>
    /// <b>Objetivo:</b> Garantir o limite de perfis por usuário definido pelo serviço.
    /// <br/><b>Critério:</b> A criação além do limite deve lançar <see cref="RegraDeNegocioException"/>.
    /// </summary>
    [Trait("Categoria", "Excecao")]
    [Fact(DisplayName = "Deve lançar exceção de regra de negócio ao ultrapassar o limite de jogadores por usuário.")]
    public async Task CriarAsync_DeveLancarExcecao_QuandoLimiteDePerfisForAtingido()
    {
        using var escopo = _ambiente.CriarEscopo();
        var usuarioService = escopo.ServiceProvider.GetRequiredService<IUsuarioService>();
        var jogadorService = escopo.ServiceProvider.GetRequiredService<IJogadorService>();

        var usuario = await usuarioService.CadastrarAsync("Vitor", "vitor@domino.local", "domino123");

        for (var indice = 1; indice <= JogadorService.MaximoDeJogadoresPorUsuario; indice++)
            await jogadorService.CriarAsync(usuario.Id, $"Jogador {indice}");

        await Assert.ThrowsAsync<RegraDeNegocioException>(() => jogadorService.CriarAsync(usuario.Id, "Excedente"));
    }

    /// <summary>
    /// <b>Objetivo:</b> Garantir que perfis não sejam criados para usuários inexistentes.
    /// <br/><b>Critério:</b> Deve lançar <see cref="RecursoNaoEncontradoException"/>.
    /// </summary>
    [Trait("Categoria", "Excecao")]
    [Fact(DisplayName = "Deve lançar exceção de recurso não encontrado ao criar jogador para usuário inexistente.")]
    public async Task CriarAsync_DeveLancarExcecao_QuandoUsuarioNaoExistir()
    {
        await Assert.ThrowsAsync<RecursoNaoEncontradoException>(() =>
            _ambiente.UsarAsync<IJogadorService, Domain.Entities.Jogador>(
                servico => servico.CriarAsync(Guid.NewGuid(), "Ana")));
    }

    /// <summary>
    /// <b>Objetivo:</b> Garantir a ordenação alfabética aplicada na consulta LINQ do repositório.
    /// <br/><b>Critério:</b> A listagem deve devolver os jogadores em ordem de nome de exibição.
    /// </summary>
    [Trait("Categoria", "Servico")]
    [Fact(DisplayName = "Deve listar os jogadores do usuário em ordem alfabética de nome de exibição.")]
    public async Task ListarPorUsuarioAsync_DeveRetornarEmOrdemAlfabetica()
    {
        var (usuario, _) = await _ambiente.CriarMesaAsync("Diego", "Ana", "Carla", "Bruno");

        var jogadores = await _ambiente.UsarAsync<IJogadorService, IReadOnlyList<Domain.Entities.Jogador>>(
            servico => servico.ListarPorUsuarioAsync(usuario.Id));

        Assert.Equal(new[] { "Ana", "Bruno", "Carla", "Diego" }, jogadores.Select(jogador => jogador.NomeExibicao));
    }

    /// <inheritdoc />
    public void Dispose() => _ambiente.Dispose();
}
