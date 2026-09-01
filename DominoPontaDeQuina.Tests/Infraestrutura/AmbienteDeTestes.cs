using DominoPontaDeQuina.App;
using DominoPontaDeQuina.Core;
using DominoPontaDeQuina.Core.Interfaces;
using DominoPontaDeQuina.Domain.Entities;
using DominoPontaDeQuina.Repository.Context;
using DominoPontaDeQuina.Repository.Interfaces;
using DominoPontaDeQuina.Repository.Repositories;
using DominoPontaDeQuina.Services;
using DominoPontaDeQuina.Services.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DominoPontaDeQuina.Tests.Infraestrutura;

/// <summary>
/// Monta um container de injeção de dependência equivalente ao configurado em <c>Program.cs</c>,
/// apontando o <see cref="DominoDbContext"/> para um banco SQLite em memória.
/// Cada instância recebe um banco novo, isolando os testes uns dos outros.
/// </summary>
public sealed class AmbienteDeTestes : IDisposable
{
    private readonly SqliteConnection _conexao;
    private readonly ServiceProvider _provedor;

    public AmbienteDeTestes()
    {
        _conexao = new SqliteConnection("Data Source=:memory:");
        _conexao.Open();

        var servicos = new ServiceCollection();

        servicos.AddDbContext<DominoDbContext>(opcoes => opcoes.UseSqlite(_conexao));

        servicos.AddScoped<IUsuarioRepository, UsuarioRepository>();
        servicos.AddScoped<IJogadorRepository, JogadorRepository>();
        servicos.AddScoped<IPartidaRepository, PartidaRepository>();
        servicos.AddScoped<IParticipacaoPartidaRepository, ParticipacaoPartidaRepository>();
        servicos.AddScoped<IRodadaRepository, RodadaRepository>();
        servicos.AddScoped<IJogadaRepository, JogadaRepository>();

        servicos.AddScoped<IHashSenhaService, HashSenhaService>();
        servicos.AddScoped<IJogo, Jogo>();
        servicos.AddScoped<IUsuarioService, UsuarioService>();
        servicos.AddScoped<IJogadorService, JogadorService>();
        servicos.AddScoped<IPartidaService, PartidaService>();
        servicos.AddScoped<IEstatisticasService, EstatisticasService>();

        servicos.AddScoped<AplicacaoConsole>();

        _provedor = servicos.BuildServiceProvider();

        using var escopo = CriarEscopo();

        escopo.ServiceProvider.GetRequiredService<DominoDbContext>().Database.Migrate();
    }

    /// <summary>
    /// Cria um novo escopo de injeção de dependência, equivalente a uma unidade de trabalho da aplicação.
    /// </summary>
    /// <returns>O escopo criado.</returns>
    public IServiceScope CriarEscopo() =>
        _provedor.CreateScope();

    /// <summary>
    /// Resolve um serviço dentro de um escopo próprio e executa a ação informada.
    /// </summary>
    /// <typeparam name="TServico">O serviço resolvido pelo container.</typeparam>
    /// <typeparam name="TResultado">O tipo devolvido pela ação.</typeparam>
    /// <param name="acao">A ação executada com o serviço resolvido.</param>
    /// <returns>O resultado da ação.</returns>
    public async Task<TResultado> UsarAsync<TServico, TResultado>(Func<TServico, Task<TResultado>> acao)
        where TServico : notnull
    {
        using var escopo = CriarEscopo();

        return await acao(escopo.ServiceProvider.GetRequiredService<TServico>());
    }

    /// <summary>
    /// Cadastra um usuário e os perfis de jogador informados, prontos para ocupar uma mesa.
    /// </summary>
    /// <param name="nomesDosJogadores">Os nomes de exibição dos jogadores criados.</param>
    /// <returns>O usuário criado e os jogadores na ordem informada.</returns>
    public async Task<(Usuario Usuario, List<Jogador> Jogadores)> CriarMesaAsync(params string[] nomesDosJogadores)
    {
        using var escopo = CriarEscopo();

        var usuarioService = escopo.ServiceProvider.GetRequiredService<IUsuarioService>();
        var jogadorService = escopo.ServiceProvider.GetRequiredService<IJogadorService>();

        var usuario = await usuarioService.CadastrarAsync("Mesa de Testes", "mesa@domino.local", "domino123");
        var jogadores = new List<Jogador>();

        foreach (var nome in nomesDosJogadores)
            jogadores.Add(await jogadorService.CriarAsync(usuario.Id, nome));

        return (usuario, jogadores);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _provedor.Dispose();
        _conexao.Dispose();
    }
}
