using DominoPontaDeQuina.Core.Exceptions;
using DominoPontaDeQuina.Domain.Entities;
using DominoPontaDeQuina.Services.Exceptions;
using DominoPontaDeQuina.Services.Interfaces;
using DominoPontaDeQuina.Services.Models;

namespace DominoPontaDeQuina.App;

/// <summary>
/// Representa o fluxo principal da aplicacao de console.
/// Todas as dependencias sao recebidas por construtor e resolvidas pelo container configurado
/// em <c>Program.cs</c>, de modo que a classe nao instancia servicos nem repositorios diretamente.
/// </summary>
public class AplicacaoConsole
{
    /// <summary>
    /// E-mail da conta usada na demonstracao do fluxo completo.
    /// </summary>
    private const string EmailDaConta = "torneio@domino.local";

    /// <summary>
    /// Senha da conta usada na demonstracao do fluxo completo.
    /// </summary>
    private const string SenhaDaConta = "domino123";

    /// <summary>
    /// Nomes dos perfis de jogador que ocupam os assentos da mesa.
    /// </summary>
    private static readonly string[] NomesDosJogadores = ["Ana", "Bruno", "Carla", "Diego"];

    private readonly IUsuarioService _usuarioService;
    private readonly IJogadorService _jogadorService;
    private readonly IPartidaService _partidaService;
    private readonly IEstatisticasService _estatisticasService;

    public AplicacaoConsole(
        IUsuarioService usuarioService,
        IJogadorService jogadorService,
        IPartidaService partidaService,
        IEstatisticasService estatisticasService)
    {
        _usuarioService = usuarioService;
        _jogadorService = jogadorService;
        _partidaService = partidaService;
        _estatisticasService = estatisticasService;
    }

    /// <summary>
    /// Executa o fluxo principal: garante a conta e os jogadores, disputa uma partida completa
    /// e apresenta o placar, o historico e o ranking a partir dos dados persistidos.
    /// </summary>
    /// <param name="pontuacaoAlvo">A pontuacao que encerra a partida.</param>
    /// <returns>O codigo de saida do processo.</returns>
    public async Task<int> ExecutarAsync(int pontuacaoAlvo)
    {
        try
        {
            Console.WriteLine("=== Domino Ponta de Quina ===");

            var usuario = await GarantirUsuarioAsync();
            var jogadores = await GarantirJogadoresAsync(usuario.Id);

            Console.WriteLine($"Conta: {usuario.Nome} <{usuario.Email}>");
            Console.WriteLine($"Mesa: {string.Join(", ", jogadores.Select(jogador => jogador.NomeExibicao))}");
            Console.WriteLine($"Pontuacao alvo: {pontuacaoAlvo}");
            Console.WriteLine();

            var resumo = await _partidaService.CriarEExecutarAsync(
                jogadores.Select(jogador => jogador.Id).ToList(),
                pontuacaoAlvo);

            ExibirResumo(resumo);
            await ExibirHistoricoAsync();
            await ExibirRankingAsync();

            return 0;
        }
        catch (DominoException excecao)
        {
            Console.Error.WriteLine($"Falha de regra de negocio: {excecao.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Recupera a conta usada na demonstracao, cadastrando-a na primeira execucao.
    /// </summary>
    /// <returns>O usuario autenticado.</returns>
    private async Task<Usuario> GarantirUsuarioAsync()
    {
        try
        {
            return await _usuarioService.AutenticarAsync(EmailDaConta, SenhaDaConta);
        }
        catch (RegraDeNegocioException)
        {
            return await _usuarioService.CadastrarAsync("Mesa do Torneio", EmailDaConta, SenhaDaConta);
        }
    }

    /// <summary>
    /// Recupera os perfis de jogador da conta, criando os que ainda nao existirem.
    /// </summary>
    /// <param name="usuarioId">O identificador do usuario dono dos perfis.</param>
    /// <returns>Os jogadores que ocuparao a mesa, na ordem dos assentos.</returns>
    private async Task<List<Jogador>> GarantirJogadoresAsync(Guid usuarioId)
    {
        var existentes = await _jogadorService.ListarPorUsuarioAsync(usuarioId);
        var jogadoresPorNome = existentes.ToDictionary(jogador => jogador.NomeExibicao);
        var jogadores = new List<Jogador>(NomesDosJogadores.Length);

        foreach (var nome in NomesDosJogadores)
        {
            jogadores.Add(jogadoresPorNome.TryGetValue(nome, out var jogador)
                ? jogador
                : await _jogadorService.CriarAsync(usuarioId, nome));
        }

        return jogadores;
    }

    /// <summary>
    /// Apresenta o placar consolidado da partida recem disputada.
    /// </summary>
    /// <param name="resumo">O resultado da partida.</param>
    private static void ExibirResumo(ResumoPartida resumo)
    {
        Console.WriteLine("--- Resultado da partida ---");
        Console.WriteLine($"Partida {resumo.PartidaId}");
        Console.WriteLine($"Rodadas: {resumo.TotalDeRodadas} | Jogadas: {resumo.TotalDeJogadas}");
        Console.WriteLine($"Time vencedor: {resumo.TimeVencedor}");

        foreach (var time in resumo.Placar)
            Console.WriteLine($"  {time.Nome}: {time.Pontuacao} ponto(s) - {string.Join(" e ", time.Jogadores)}");

        Console.WriteLine();
    }

    /// <summary>
    /// Apresenta as ultimas partidas registradas na base de dados.
    /// </summary>
    private async Task ExibirHistoricoAsync()
    {
        var partidas = await _partidaService.ListarUltimasAsync(5);

        Console.WriteLine("--- Ultimas partidas ---");

        foreach (var partida in partidas)
            Console.WriteLine($"  {partida.IniciadoEm:dd/MM/yyyy HH:mm:ss} | alvo {partida.PontuacaoAlvo} | {partida.Status}");

        Console.WriteLine();
    }

    /// <summary>
    /// Apresenta o ranking dos jogadores a partir das consultas dos repositorios.
    /// </summary>
    private async Task ExibirRankingAsync()
    {
        var ranking = await _estatisticasService.ObterRankingAsync();

        Console.WriteLine("--- Ranking de jogadores ---");

        foreach (var estatisticas in ranking)
        {
            Console.WriteLine(
                $"  {estatisticas.NomeExibicao,-8} vitorias {estatisticas.PartidasVencidas,3}/{estatisticas.PartidasDisputadas,-3} " +
                $"rodadas {estatisticas.RodadasVencidas,3} pontos {estatisticas.PontuacaoTotal,4} " +
                $"aproveitamento {estatisticas.Aproveitamento:P0}");
        }

        Console.WriteLine();
    }
}
