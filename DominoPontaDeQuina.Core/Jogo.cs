using DominoPontaDeQuina.Core.Enums;
using DominoPontaDeQuina.Core.Exceptions;
using DominoPontaDeQuina.Core.Interfaces;
using DominoPontaDeQuina.Core.Models;
using System.Collections.ObjectModel;

namespace DominoPontaDeQuina.Core;

/// <summary>
/// Controla o fluxo principal no topo da hierarquia Partida -> Rodadas -> Jogadas.
/// Neste nivel fica a orquestracao da partida atual, da sequencia de rodadas e da execucao das jogadas.
/// </summary>
public class Jogo() : IJogo
{
    /// <summary>
    /// Pontuacao alvo adotada quando a partida nao define uma pontuacao especifica.
    /// </summary>
    public const int PontuacaoAlvoPadrao = 50;

    /// <summary>
    /// Quantidade minima de jogadores necessaria para formar os dois times da partida.
    /// </summary>
    private const int MinimoDeJogadores = 2;

    /// <summary>
    /// Mantem internamente o historico das partidas iniciadas pelo jogo.
    /// </summary>
    Stack<Partida> _partidas = [];

    /// <inheritdoc />
    public ReadOnlyCollection<Partida> HistoricoPartidas => _partidas.ToList().AsReadOnly();

    /// <inheritdoc />
    public Partida? PartidaAtual => _partidas.TryPeek(out var partidaAtual) ? partidaAtual : null;

    /// <summary>
    /// Registra os times da partida atual usando a formacao padrao de quatro jogadores.
    /// A partida e organizada em dois times com dois jogadores cada, formato em que a composicao
    /// dos times define a disputa em duplas.
    /// </summary>
    /// <exception cref="PartidaInvalidaException">Quando nao houver partida atual para receber os times.</exception>
    public Task RegistrarTimesAsync() =>
        RegistrarTimesAsync(CriarJogadoresPadrao());

    /// <summary>
    /// Registra os times da partida atual distribuindo os jogadores informados alternadamente.
    /// A ordem recebida representa o assento na mesa, de forma que jogadores vizinhos fiquem em times opostos.
    /// </summary>
    /// <param name="jogadores">Os jogadores participantes da partida.</param>
    /// <exception cref="PartidaInvalidaException">Quando nao houver partida atual ou a formacao dos times for invalida.</exception>
    public Task RegistrarTimesAsync(IReadOnlyList<Jogador> jogadores)
    {
        if (PartidaAtual is null)
            throw new PartidaInvalidaException("Nao ha partida atual para registrar os times.");

        ValidarJogadores(jogadores);

        if (PartidaAtual.Times.Count > 0)
            return Task.CompletedTask;

        var primeiroTime = new Time("Time A");
        var segundoTime = new Time("Time B");

        for (var assento = 0; assento < jogadores.Count; assento++)
        {
            var time = assento % 2 == 0 ? primeiroTime : segundoTime;
            time.AdicionarJogador(jogadores[assento]);
        }

        PartidaAtual.Times.Add(primeiroTime);
        PartidaAtual.Times.Add(segundoTime);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Inicia uma nova partida com a formacao padrao e executa suas rodadas ate a finalizacao.
    /// </summary>
    /// <exception cref="PartidaInvalidaException">Quando a partida atual ainda estiver em andamento.</exception>
    public async Task IniciarNovaPartida()
    {
        await IniciarPartidaAsync(CriarJogadoresPadrao());

        while (PartidaAtual?.Status is StatusPartida.EmAndamento)
        {
            await ExecutarRodadaAtualAsync();
            AvancarParaProximaRodada();
        }
    }

    /// <inheritdoc />
    /// <exception cref="PartidaInvalidaException">Quando a partida atual ainda estiver em andamento ou a formacao dos times for invalida.</exception>
    public async Task IniciarPartidaAsync(IReadOnlyList<Jogador> jogadores, int pontuacaoAlvo = PontuacaoAlvoPadrao)
    {
        if (PartidaAtual?.Status is StatusPartida.EmAndamento)
            throw new PartidaInvalidaException("Nao e possivel iniciar uma nova partida enquanto a partida atual estiver em andamento.");

        ValidarJogadores(jogadores);

        _partidas.Push(new Partida(pontuacaoAlvo));

        await RegistrarTimesAsync(jogadores);

        IniciarRodada();
    }

    /// <inheritdoc />
    /// <exception cref="PartidaInvalidaException">Quando a partida nao estiver em andamento.</exception>
    /// <exception cref="RodadaInvalidaException">Quando nao houver rodada atual para executar.</exception>
    public async Task<Rodada> ExecutarRodadaAtualAsync()
    {
        if (PartidaAtual?.Status is not StatusPartida.EmAndamento)
            throw new PartidaInvalidaException("Nao e possivel executar uma rodada em uma partida que nao esta em andamento.");

        var rodadaAtual = PartidaAtual.RodadaAtual
            ?? throw new RodadaInvalidaException("Nao ha rodada atual para executar.");

        await ExecutarRodadaPartidaAsync();

        return rodadaAtual;
    }

    /// <inheritdoc />
    /// <exception cref="PartidaInvalidaException">Quando a partida nao estiver em andamento.</exception>
    public bool AvancarParaProximaRodada()
    {
        if (PartidaAtual?.Status is not StatusPartida.EmAndamento)
            throw new PartidaInvalidaException("Nao e possivel avancar rodadas em uma partida que nao esta em andamento.");

        if (PartidaAtual.VerificaPontuacaoAlvoAtingida())
        {
            PartidaAtual.FinalizarPartida();
            return false;
        }

        IniciarRodada();

        return true;
    }

    /// <summary>
    /// Executa o fluxo da rodada atual enquanto ela estiver em andamento.
    /// Ao final da rodada, a pontuacao apurada e creditada aos times dos jogadores.
    /// </summary>
    public async Task ExecutarRodadaPartidaAsync()
    {
        if (PartidaAtual?.Status is not StatusPartida.EmAndamento)
            return;
        if (PartidaAtual.RodadaAtual?.Status is not StatusRodada.EmAndamento)
            return;

        var rodadaAtual = PartidaAtual.RodadaAtual;

        while (rodadaAtual.Status is StatusRodada.EmAndamento)
        {
            await ExecutarJogadaAsync();
            rodadaAtual.VerificarBatida();
            rodadaAtual.VerificarTabuleiroTravado();
        }

        ContabilizarPontuacaoDaRodada(rodadaAtual);
    }

    /// <summary>
    /// Executa a jogada do jogador atual na rodada em andamento.
    /// </summary>
    /// <exception cref="PartidaInvalidaException">Quando a partida nao estiver em andamento.</exception>
    /// <exception cref="RodadaInvalidaException">Quando a rodada nao estiver em andamento.</exception>
    /// <exception cref="JogadaInvalidaException">Quando a jogada escolhida nao for valida.</exception>
    public async Task ExecutarJogadaAsync()
    {
        if (PartidaAtual?.Status is not StatusPartida.EmAndamento)
            throw new PartidaInvalidaException("Nao e possivel executar uma jogada em uma partida que nao esta em andamento.");
        if (PartidaAtual.RodadaAtual?.Status is not StatusRodada.EmAndamento)
            throw new RodadaInvalidaException("Nao e possivel executar uma jogada em uma rodada que nao esta em andamento.");

        var jogadorAtual = PartidaAtual.RodadaAtual.JogadorAtual;
        var jogada = await GetJogadaAsync();

        if (!ValidarJogada(jogada))
        {
            jogadorAtual.DefazerJogada(jogada);
            jogada.MarcarComoInvalida();
            throw new JogadaInvalidaException("A jogada realizada e invalida.");
        }

        PartidaAtual.RodadaAtual.RegistrarJogada(jogada);
    }

    /// <summary>
    /// Obtem a jogada definida pelo jogador atual com base no estado do tabuleiro.
    /// </summary>
    /// <returns>A jogada escolhida pelo jogador atual.</returns>
    /// <exception cref="RodadaInvalidaException">Quando nao houver rodada atual na partida.</exception>
    public Task<Jogada> GetJogadaAsync()
    {
        if (PartidaAtual?.RodadaAtual is null)
            throw new RodadaInvalidaException("Nao ha rodada atual para obter jogada.");

        var jogadorAtual = PartidaAtual.RodadaAtual.JogadorAtual;
        return Task.FromResult(jogadorAtual.GetJogada(PartidaAtual.RodadaAtual.Tabuleiro));
    }

    /// <summary>
    /// Valida a jogada no contexto da rodada atual.
    /// Passar a vez e sempre valido; nas demais jogadas a peca precisa encaixar na ponta escolhida.
    /// </summary>
    /// <param name="jogada">A jogada a ser validada.</param>
    /// <returns><see langword="true"/> quando a jogada for valida; caso contrario, <see langword="false"/>.</returns>
    public bool ValidarJogada(Jogada jogada)
    {
        if (jogada is null)
            return false;

        if (jogada.EhPassarVez())
            return true;

        if (jogada.Peca is null || jogada.Lado is null)
            return false;

        var tabuleiro = PartidaAtual?.RodadaAtual?.Tabuleiro;

        return tabuleiro is not null && tabuleiro.PodeColar(jogada.Peca.Value, jogada.Lado.Value);
    }

    /// <summary>
    /// Cria a formacao padrao de quatro jogadores usada quando nenhum jogador e informado.
    /// </summary>
    /// <returns>Os jogadores padrao, em ordem de assento na mesa.</returns>
    private static IReadOnlyList<Jogador> CriarJogadoresPadrao() =>
    [
        new Jogador("Jogador 1"),
        new Jogador("Jogador 2"),
        new Jogador("Jogador 3"),
        new Jogador("Jogador 4")
    ];

    /// <summary>
    /// Valida a formacao de jogadores recebida para a partida.
    /// </summary>
    /// <param name="jogadores">Os jogadores participantes da partida.</param>
    /// <exception cref="PartidaInvalidaException">Quando a quantidade de jogadores nao permitir formar dois times equilibrados.</exception>
    private static void ValidarJogadores(IReadOnlyList<Jogador> jogadores)
    {
        if (jogadores is null || jogadores.Count < MinimoDeJogadores)
            throw new PartidaInvalidaException("Uma partida precisa de pelo menos dois jogadores.");

        if (jogadores.Count % 2 != 0)
            throw new PartidaInvalidaException("Uma partida precisa de uma quantidade par de jogadores para formar os dois times.");
    }

    /// <summary>
    /// Inicia uma nova rodada na partida atual e distribui as pecas entre os jogadores.
    /// </summary>
    private void IniciarRodada()
    {
        if (PartidaAtual is null)
            throw new PartidaInvalidaException("Nao ha partida atual para iniciar uma rodada.");

        PartidaAtual.IniciarNovaRodada();
        PartidaAtual.RodadaAtual?.Iniciar(ObterJogadoresDaPartida(), ObterRodadaAnterior());
    }

    /// <summary>
    /// Credita aos times a pontuacao apurada pelos jogadores na rodada encerrada.
    /// </summary>
    /// <param name="rodada">A rodada ja finalizada.</param>
    private void ContabilizarPontuacaoDaRodada(Rodada rodada)
    {
        if (PartidaAtual is null)
            return;

        foreach (var pontuacaoDoJogador in rodada.PontuacaoJogadores)
        {
            var time = PartidaAtual.Times
                .FirstOrDefault(time => time.PossuiJogador(pontuacaoDoJogador.Key));

            time?.SomarPontos(pontuacaoDoJogador.Value);
        }
    }

    /// <summary>
    /// Obtem os jogadores registrados nos times da partida atual.
    /// </summary>
    /// <returns>A colecao somente leitura dos jogadores da partida.</returns>
    /// <exception cref="PartidaInvalidaException">Quando nao houver partida atual.</exception>
    private ReadOnlyCollection<Jogador> ObterJogadoresDaPartida()
    {
        if (PartidaAtual is null)
            throw new PartidaInvalidaException("Nao ha partida atual para obter jogadores.");

        return PartidaAtual.Times
            .SelectMany(time => time.Jogadores)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Obtem a rodada anterior a rodada atual, quando houver.
    /// </summary>
    /// <returns>A rodada anterior, ou <see langword="null"/> quando a rodada atual for a primeira da partida.</returns>
    private Rodada? ObterRodadaAnterior()
    {
        if (PartidaAtual is null || PartidaAtual.HistoricoRodadas.Count < 2)
            return null;

        return PartidaAtual.HistoricoRodadas[1];
    }
}
