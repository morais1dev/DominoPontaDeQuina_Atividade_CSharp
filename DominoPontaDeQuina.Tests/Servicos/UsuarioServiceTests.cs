using DominoPontaDeQuina.Services.Exceptions;
using DominoPontaDeQuina.Services.Interfaces;
using DominoPontaDeQuina.Tests.Infraestrutura;
using Microsoft.Extensions.DependencyInjection;

namespace DominoPontaDeQuina.Tests.Servicos;

/// <summary>
/// Testes das regras de uso da conta de usuário, resolvendo o serviço pelo container de injeção de dependência.
/// </summary>
public class UsuarioServiceTests : IDisposable
{
    private readonly AmbienteDeTestes _ambiente = new();

    /// <summary>
    /// <b>Objetivo:</b> Garantir que a senha nunca seja armazenada em texto puro.
    /// <br/><b>Critério:</b> O hash persistido deve ser diferente da senha e deve validar a autenticação.
    /// </summary>
    [Trait("Categoria", "Servico")]
    [Fact(DisplayName = "Deve armazenar a senha do usuário como hash e permitir a autenticação.")]
    public async Task CadastrarAsync_DeveArmazenarSenhaComoHash()
    {
        var usuario = await _ambiente.UsarAsync<IUsuarioService, Domain.Entities.Usuario>(
            servico => servico.CadastrarAsync("Vitor", "vitor@domino.local", "domino123"));

        Assert.NotEqual("domino123", usuario.HashSenha);
        Assert.NotEmpty(usuario.HashSenha);

        var autenticado = await _ambiente.UsarAsync<IUsuarioService, Domain.Entities.Usuario>(
            servico => servico.AutenticarAsync("vitor@domino.local", "domino123"));

        Assert.Equal(usuario.Id, autenticado.Id);
    }

    /// <summary>
    /// <b>Objetivo:</b> Garantir que o e-mail seja tratado sem diferenciar caixa alta e baixa.
    /// <br/><b>Critério:</b> A autenticação deve funcionar com o e-mail informado em outra caixa.
    /// </summary>
    [Trait("Categoria", "Servico")]
    [Fact(DisplayName = "Deve autenticar o usuário ignorando a caixa do e-mail informado.")]
    public async Task AutenticarAsync_DeveIgnorarCaixaDoEmail()
    {
        var usuario = await _ambiente.UsarAsync<IUsuarioService, Domain.Entities.Usuario>(
            servico => servico.CadastrarAsync("Vitor", "Vitor@Domino.Local", "domino123"));

        var autenticado = await _ambiente.UsarAsync<IUsuarioService, Domain.Entities.Usuario>(
            servico => servico.AutenticarAsync("VITOR@DOMINO.LOCAL", "domino123"));

        Assert.Equal(usuario.Id, autenticado.Id);
        Assert.Equal("vitor@domino.local", autenticado.Email);
    }

    /// <summary>
    /// <b>Objetivo:</b> Garantir que o mesmo e-mail não seja cadastrado duas vezes.
    /// <br/><b>Critério:</b> A segunda tentativa deve lançar <see cref="RegraDeNegocioException"/>.
    /// </summary>
    [Trait("Categoria", "Excecao")]
    [Fact(DisplayName = "Deve lançar exceção de regra de negócio ao cadastrar e-mail já utilizado.")]
    public async Task CadastrarAsync_DeveLancarExcecao_QuandoEmailJaCadastrado()
    {
        using var escopo = _ambiente.CriarEscopo();
        var servico = escopo.ServiceProvider.GetRequiredService<IUsuarioService>();

        await servico.CadastrarAsync("Vitor", "vitor@domino.local", "domino123");

        await Assert.ThrowsAsync<RegraDeNegocioException>(
            () => servico.CadastrarAsync("Outro", "vitor@domino.local", "domino123"));
    }

    /// <summary>
    /// <b>Objetivo:</b> Garantir a validação do formato do e-mail informado.
    /// <br/><b>Critério:</b> Deve lançar <see cref="RegraDeNegocioException"/> para e-mails mal formados.
    /// </summary>
    [Trait("Categoria", "Excecao")]
    [Theory(DisplayName = "Deve lançar exceção de regra de negócio ao cadastrar e-mail com formato inválido.")]
    [InlineData("sem-arroba")]
    [InlineData("@dominio.local")]
    [InlineData("usuario@")]
    public async Task CadastrarAsync_DeveLancarExcecao_QuandoEmailInvalido(string email)
    {
        await Assert.ThrowsAsync<RegraDeNegocioException>(() =>
            _ambiente.UsarAsync<IUsuarioService, Domain.Entities.Usuario>(
                servico => servico.CadastrarAsync("Vitor", email, "domino123")));
    }

    /// <summary>
    /// <b>Objetivo:</b> Garantir o tamanho mínimo da senha exigido pelo serviço.
    /// <br/><b>Critério:</b> Deve lançar <see cref="RegraDeNegocioException"/> para senhas curtas.
    /// </summary>
    [Trait("Categoria", "Excecao")]
    [Fact(DisplayName = "Deve lançar exceção de regra de negócio ao cadastrar senha menor que o mínimo exigido.")]
    public async Task CadastrarAsync_DeveLancarExcecao_QuandoSenhaForCurta()
    {
        await Assert.ThrowsAsync<RegraDeNegocioException>(() =>
            _ambiente.UsarAsync<IUsuarioService, Domain.Entities.Usuario>(
                servico => servico.CadastrarAsync("Vitor", "vitor@domino.local", "123")));
    }

    /// <summary>
    /// <b>Objetivo:</b> Garantir que credenciais inválidas não autentiquem o usuário.
    /// <br/><b>Critério:</b> Deve lançar <see cref="RegraDeNegocioException"/> quando a senha não conferir.
    /// </summary>
    [Trait("Categoria", "Excecao")]
    [Fact(DisplayName = "Deve lançar exceção de regra de negócio ao autenticar com senha incorreta.")]
    public async Task AutenticarAsync_DeveLancarExcecao_QuandoSenhaIncorreta()
    {
        using var escopo = _ambiente.CriarEscopo();
        var servico = escopo.ServiceProvider.GetRequiredService<IUsuarioService>();

        await servico.CadastrarAsync("Vitor", "vitor@domino.local", "domino123");

        await Assert.ThrowsAsync<RegraDeNegocioException>(
            () => servico.AutenticarAsync("vitor@domino.local", "senha-errada"));
    }

    /// <summary>
    /// <b>Objetivo:</b> Garantir a troca de senha mediante confirmação da senha atual.
    /// <br/><b>Critério:</b> A autenticação deve passar a funcionar somente com a nova senha.
    /// </summary>
    [Trait("Categoria", "Servico")]
    [Fact(DisplayName = "Deve alterar a senha do usuário quando a senha atual for confirmada.")]
    public async Task AlterarSenhaAsync_DeveTrocarSenha_QuandoSenhaAtualConferir()
    {
        using var escopo = _ambiente.CriarEscopo();
        var servico = escopo.ServiceProvider.GetRequiredService<IUsuarioService>();

        var usuario = await servico.CadastrarAsync("Vitor", "vitor@domino.local", "domino123");

        await servico.AlterarSenhaAsync(usuario.Id, "domino123", "quina2026");

        var autenticado = await servico.AutenticarAsync("vitor@domino.local", "quina2026");

        Assert.Equal(usuario.Id, autenticado.Id);
        await Assert.ThrowsAsync<RegraDeNegocioException>(
            () => servico.AutenticarAsync("vitor@domino.local", "domino123"));
    }

    /// <inheritdoc />
    public void Dispose() => _ambiente.Dispose();
}
