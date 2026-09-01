using DominoPontaDeQuina.Core;
using DominoPontaDeQuina.Core.Interfaces;
using DominoPontaDeQuina.Domain.Entities;
using DominoPontaDeQuina.Domain.Enums;
using DominoPontaDeQuina.Repository.Interfaces;
using DominoPontaDeQuina.Services.Exceptions;
using DominoPontaDeQuina.Services.Interfaces;
using DominoPontaDeQuina.Services.Models;
using JogadaDoJogo = DominoPontaDeQuina.Core.Models.Jogada;
using JogadorDoJogo = DominoPontaDeQuina.Core.Models.Jogador;
using LadoDoJogo = DominoPontaDeQuina.Core.Enums.LadoTabuleiro;
using RodadaDoJogo = DominoPontaDeQuina.Core.Models.Rodada;
using TipoFinalizacaoDoJogo = DominoPontaDeQuina.Core.Enums.TipoFinalizacaoRodada;

namespace DominoPontaDeQuina.Services;

/// <inheritdoc cref="IPartidaService"/>
public class PartidaService : IPartidaService
{
    /// <summary>
    /// Quantidade minima de jogadores necessaria para formar os dois times.
    /// </summary>
    private const int MinimoDeJogadores = 2;

    /// <summary>
    /// Quantidade maxima de jogadores aceita em uma mesa.
    /// </summary>
    private const int MaximoDeJogadores = 4;

    /// <summary>
    /// Limite de rodadas usado como protecao contra partidas que nao evoluem em pontuacao.
    /// </summary>
    private const int MaximoDeRodadas = 500;

    /// <summary>
    /// Nomes dos times criados para a partida, na ordem de distribuicao dos assentos.
    /// </summary>
    private static readonly string[] NomesDosTimes = ["Time A", "Time B"];

    private readonly IPartidaRepository _partidaRepository;
    private readonly IParticipacaoPartidaRepository _participacaoRepository;
    private readonly IRodadaRepository _rodadaRepository;
    private readonly IJogadaRepository _jogadaRepository;
    private readonly IJogadorRepository _jogadorRepository;
    private readonly IJogo _jogo;

    public PartidaService(
        IPartidaRepository partidaRepository,
        IParticipacaoPartidaRepository participacaoRepository,
        IRodadaRepository rodadaRepository,
        IJogadaRepository jogadaRepository,
        IJogadorRepository jogadorRepository,
        IJogo jogo)
    {
        _partidaRepository = partidaRepository;
        _participacaoRepository = participacaoRepository;
        _rodadaRepository = rodadaRepository;
        _jogadaRepository = jogadaRepository;
        _jogadorRepository = jogadorRepository;
        _jogo = jogo;
    }

    /// <inheritdoc />
    /// <exception cref="RegraDeNegocioException">Quando a formacao de jogadores ou a pontuacao alvo for invalida.</exception>
    public async Task<Partida> CriarAsync(IReadOnlyList<Guid> jogadoresIds, int pontuacaoAlvo = Jogo.PontuacaoAlvoPadrao)
    {
        ValidarPontuacaoAlvo(pontuacaoAlvo);

        var jogadores = await ObterJogadoresDaMesaAsync(jogadoresIds);

        var partida = new Partida
        {
            PontuacaoAlvo = pontuacaoAlvo,
            Status = StatusPartida.Aguardando
        };

        var times = NomesDosTimes
            .Select(nome => new TimePartida { Nome = nome })
            .ToList();

        foreach (var time in times)
            partida.Times.Add(time);

        for (var assento = 0; assento < jogadores.Count; assento++)
        {
            var participacao = new ParticipacaoPartida
            {
                JogadorId = jogadores[assento].Id,
                Posicao = assento
            };

            times[assento % times.Count].Participacoes.Add(participacao);
            partida.Participacoes.Add(participacao);
        }

        return await _partidaRepository.AdicionarAsync(partida);
    }

    /// <inheritdoc />
    /// <exception cref="RecursoNaoEncontradoException">Quando a partida nao existir.</exception>
    /// <exception cref="RegraDeNegocioException">Quando a partida nao estiver aguardando execucao.</exception>
    public async Task<ResumoPartida> ExecutarAsync(Guid partidaId)
    {
        var partida = await ObterAsync(partidaId);

        if (partida.Status is not StatusPartida.Aguardando)
            throw new RegraDeNegocioException($"A partida {partidaId} nao esta aguardando execucao.");

        var participacoes = partida.Participacoes
            .OrderBy(participacao => participacao.Posicao)
            .ToList();

        var jogadoresDoJogo = participacoes
            .Select(participacao => new JogadorDoJogo(participacao.Jogador.NomeExibicao, participacao.JogadorId))
            .ToList();

        await _jogo.IniciarPartidaAsync(jogadoresDoJogo, partida.PontuacaoAlvo);

        partida.Status = StatusPartida.EmAndamento;
        await _partidaRepository.AtualizarAsync(partida);

        var pontuacaoPorJogador = participacoes.ToDictionary(participacao => participacao.JogadorId, _ => 0);
        var totalDeJogadas = 0;
        var numeroDaRodada = 0;
        var partidaEmAndamento = true;

        while (partidaEmAndamento)
        {
            if (++numeroDaRodada > MaximoDeRodadas)
                throw new RegraDeNegocioException($"A partida {partidaId} ultrapassou o limite de {MaximoDeRodadas} rodadas.");

            var rodadaDoJogo = await _jogo.ExecutarRodadaAtualAsync();

            totalDeJogadas += await PersistirRodadaAsync(partida.Id, numeroDaRodada, rodadaDoJogo);
            AcumularPontuacao(pontuacaoPorJogador, rodadaDoJogo);

            partidaEmAndamento = _jogo.AvancarParaProximaRodada();
        }

        return await FinalizarAsync(partida, participacoes, pontuacaoPorJogador, numeroDaRodada, totalDeJogadas);
    }

    /// <inheritdoc />
    public async Task<ResumoPartida> CriarEExecutarAsync(IReadOnlyList<Guid> jogadoresIds, int pontuacaoAlvo = Jogo.PontuacaoAlvoPadrao)
    {
        var partida = await CriarAsync(jogadoresIds, pontuacaoAlvo);

        return await ExecutarAsync(partida.Id);
    }

    /// <inheritdoc />
    /// <exception cref="RecursoNaoEncontradoException">Quando a partida nao existir.</exception>
    public async Task<Partida> ObterAsync(Guid partidaId)
    {
        return await _partidaRepository.ObterCompletaPorIdAsync(partidaId)
            ?? throw RecursoNaoEncontradoException.Para("partida", partidaId);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Partida>> ListarUltimasAsync(int quantidade) =>
        await _partidaRepository.ListarUltimasAsync(quantidade < 1 ? 1 : quantidade);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Partida>> ListarPorJogadorAsync(Guid jogadorId) =>
        await _partidaRepository.ListarPorJogadorAsync(jogadorId);

    /// <inheritdoc />
    /// <exception cref="RecursoNaoEncontradoException">Quando a partida nao existir.</exception>
    /// <exception cref="RegraDeNegocioException">Quando a partida ja estiver finalizada.</exception>
    public async Task CancelarAsync(Guid partidaId)
    {
        var partida = await ObterAsync(partidaId);

        if (partida.Status is StatusPartida.Finalizada)
            throw new RegraDeNegocioException($"A partida {partidaId} ja foi finalizada e nao pode ser cancelada.");

        partida.Status = StatusPartida.Cancelada;
        partida.FinalizadoEm = DateTime.UtcNow;

        await _partidaRepository.AtualizarAsync(partida);
    }

    /// <summary>
    /// Obtem e valida os jogadores que ocuparao os assentos da mesa.
    /// </summary>
    /// <param name="jogadoresIds">Os identificadores informados, na ordem dos assentos.</param>
    /// <returns>Os jogadores na mesma ordem dos identificadores.</returns>
    /// <exception cref="RegraDeNegocioException">Quando a formacao for invalida.</exception>
    /// <exception cref="RecursoNaoEncontradoException">Quando algum jogador informado nao existir.</exception>
    private async Task<List<Jogador>> ObterJogadoresDaMesaAsync(IReadOnlyList<Guid> jogadoresIds)
    {
        if (jogadoresIds is null || jogadoresIds.Count < MinimoDeJogadores || jogadoresIds.Count > MaximoDeJogadores)
            throw new RegraDeNegocioException($"Uma partida precisa de {MinimoDeJogadores} a {MaximoDeJogadores} jogadores.");

        if (jogadoresIds.Count % 2 != 0)
            throw new RegraDeNegocioException("Uma partida precisa de uma quantidade par de jogadores para formar os dois times.");

        if (jogadoresIds.Distinct().Count() != jogadoresIds.Count)
            throw new RegraDeNegocioException("Um mesmo jogador nao pode ocupar mais de um assento na mesa.");

        var jogadoresEncontrados = await _jogadorRepository.ListarPorIdsAsync(jogadoresIds);
        var jogadoresPorId = jogadoresEncontrados.ToDictionary(jogador => jogador.Id);

        return jogadoresIds
            .Select(id => jogadoresPorId.TryGetValue(id, out var jogador)
                ? jogador
                : throw RecursoNaoEncontradoException.Para("jogador", id))
            .ToList();
    }

    /// <summary>
    /// Persiste a rodada executada pelo motor do jogo junto com todas as suas jogadas.
    /// </summary>
    /// <param name="partidaId">O identificador da partida.</param>
    /// <param name="numero">O numero sequencial da rodada dentro da partida.</param>
    /// <param name="rodadaDoJogo">A rodada ja finalizada pelo motor do jogo.</param>
    /// <returns>A quantidade de jogadas persistidas.</returns>
    private async Task<int> PersistirRodadaAsync(Guid partidaId, int numero, RodadaDoJogo rodadaDoJogo)
    {
        var vencedor = rodadaDoJogo.GetVencedor();

        var rodada = new Rodada
        {
            PartidaId = partidaId,
            Numero = numero,
            Status = StatusRodada.Finalizada,
            TipoFinalizacao = MapearTipoFinalizacao(rodadaDoJogo.TipoFinalizacao),
            JogadorVencedorId = vencedor?.Id,
            PontuacaoVencedor = ObterPontuacaoDoJogador(rodadaDoJogo, vencedor),
            FinalizadaEm = DateTime.UtcNow
        };

        await _rodadaRepository.AdicionarAsync(rodada);

        var jogadas = rodadaDoJogo.HistoricoJogadas
            .Reverse()
            .Select((jogadaDoJogo, indice) => MapearJogada(rodada.Id, indice + 1, jogadaDoJogo))
            .ToList();

        await _jogadaRepository.AdicionarVariasAsync(jogadas);

        return jogadas.Count;
    }

    /// <summary>
    /// Converte a jogada executada pelo motor do jogo na entidade persistente correspondente.
    /// </summary>
    /// <param name="rodadaId">O identificador da rodada persistida.</param>
    /// <param name="sequencia">A posicao da jogada dentro da rodada.</param>
    /// <param name="jogadaDoJogo">A jogada executada.</param>
    /// <returns>A jogada pronta para ser persistida.</returns>
    private static Jogada MapearJogada(Guid rodadaId, int sequencia, JogadaDoJogo jogadaDoJogo) =>
        new()
        {
            RodadaId = rodadaId,
            JogadorId = jogadaDoJogo.Jogador.Id,
            Sequencia = sequencia,
            PecaValorA = jogadaDoJogo.Peca?.ValorA,
            PecaValorB = jogadaDoJogo.Peca?.ValorB,
            Lado = MapearLado(jogadaDoJogo.Lado),
            PassouVez = jogadaDoJogo.EhPassarVez(),
            PontosGerados = jogadaDoJogo.PontosGerados
        };

    /// <summary>
    /// Acumula na tabela de pontuacao da partida os pontos apurados por jogador na rodada.
    /// </summary>
    /// <param name="pontuacaoPorJogador">A pontuacao acumulada por jogador.</param>
    /// <param name="rodadaDoJogo">A rodada ja finalizada.</param>
    private static void AcumularPontuacao(Dictionary<Guid, int> pontuacaoPorJogador, RodadaDoJogo rodadaDoJogo)
    {
        foreach (var pontuacao in rodadaDoJogo.PontuacaoJogadores)
        {
            if (pontuacaoPorJogador.ContainsKey(pontuacao.Key.Id))
                pontuacaoPorJogador[pontuacao.Key.Id] += pontuacao.Value;
        }
    }

    /// <summary>
    /// Consolida o resultado da partida nos times, nas participacoes e na propria partida.
    /// </summary>
    /// <param name="partida">A partida executada.</param>
    /// <param name="participacoes">As participacoes da partida em ordem de assento.</param>
    /// <param name="pontuacaoPorJogador">A pontuacao acumulada por jogador.</param>
    /// <param name="totalDeRodadas">A quantidade de rodadas disputadas.</param>
    /// <param name="totalDeJogadas">A quantidade de jogadas registradas.</param>
    /// <returns>O resultado consolidado da partida.</returns>
    private async Task<ResumoPartida> FinalizarAsync(
        Partida partida,
        List<ParticipacaoPartida> participacoes,
        Dictionary<Guid, int> pontuacaoPorJogador,
        int totalDeRodadas,
        int totalDeJogadas)
    {
        var partidaDoJogo = _jogo.PartidaAtual
            ?? throw new RegraDeNegocioException("O motor do jogo nao possui uma partida para consolidar.");

        var pontuacaoPorTime = partidaDoJogo.Times
            .ToDictionary(time => time.Nome, time => time.Pontuacao);

        var nomeDoTimeVencedor = partidaDoJogo.GetTimeVencedor()?.Nome ?? string.Empty;

        foreach (var time in partida.Times)
        {
            time.Pontuacao = pontuacaoPorTime.TryGetValue(time.Nome, out var pontuacao) ? pontuacao : 0;
            time.Vencedor = time.Nome == nomeDoTimeVencedor;
        }

        foreach (var participacao in participacoes)
        {
            participacao.Pontuacao = pontuacaoPorJogador[participacao.JogadorId];
            participacao.Vencedor = partida.Times.Any(time => time.Id == participacao.TimePartidaId && time.Vencedor);

            await _participacaoRepository.AtualizarAsync(participacao);
        }

        partida.Status = StatusPartida.Finalizada;
        partida.FinalizadoEm = DateTime.UtcNow;

        await _partidaRepository.AtualizarAsync(partida);

        return MontarResumo(partida, participacoes, nomeDoTimeVencedor, totalDeRodadas, totalDeJogadas);
    }

    /// <summary>
    /// Monta o resumo apresentado ao chamador apos a execucao da partida.
    /// </summary>
    /// <param name="partida">A partida ja finalizada.</param>
    /// <param name="participacoes">As participacoes da partida.</param>
    /// <param name="nomeDoTimeVencedor">O nome do time vencedor.</param>
    /// <param name="totalDeRodadas">A quantidade de rodadas disputadas.</param>
    /// <param name="totalDeJogadas">A quantidade de jogadas registradas.</param>
    /// <returns>O resultado consolidado da partida.</returns>
    private static ResumoPartida MontarResumo(
        Partida partida,
        List<ParticipacaoPartida> participacoes,
        string nomeDoTimeVencedor,
        int totalDeRodadas,
        int totalDeJogadas)
    {
        var placar = partida.Times
            .OrderByDescending(time => time.Pontuacao)
            .Select(time => new PlacarTime(
                time.Nome,
                time.Pontuacao,
                time.Vencedor,
                participacoes
                    .Where(participacao => participacao.TimePartidaId == time.Id)
                    .Select(participacao => participacao.Jogador.NomeExibicao)
                    .ToList()))
            .ToList();

        return new ResumoPartida(
            partida.Id,
            partida.PontuacaoAlvo,
            totalDeRodadas,
            totalDeJogadas,
            nomeDoTimeVencedor,
            placar);
    }

    /// <summary>
    /// Obtem a pontuacao apurada pelo jogador informado dentro da rodada.
    /// </summary>
    /// <param name="rodadaDoJogo">A rodada ja finalizada.</param>
    /// <param name="jogador">O jogador consultado.</param>
    /// <returns>A pontuacao do jogador na rodada.</returns>
    private static int ObterPontuacaoDoJogador(RodadaDoJogo rodadaDoJogo, JogadorDoJogo? jogador) =>
        jogador is not null && rodadaDoJogo.PontuacaoJogadores.TryGetValue(jogador, out var pontuacao)
            ? pontuacao
            : 0;

    /// <summary>
    /// Converte o motivo de encerramento da rodada para o enum persistente.
    /// </summary>
    /// <param name="tipoFinalizacao">O motivo apurado pelo motor do jogo.</param>
    /// <returns>O motivo correspondente no modelo persistente.</returns>
    private static TipoFinalizacaoRodada? MapearTipoFinalizacao(TipoFinalizacaoDoJogo? tipoFinalizacao) =>
        tipoFinalizacao switch
        {
            TipoFinalizacaoDoJogo.JogadorBateu => TipoFinalizacaoRodada.JogadorBateu,
            TipoFinalizacaoDoJogo.TabuleiroTravado => TipoFinalizacaoRodada.TabuleiroTravado,
            _ => null
        };

    /// <summary>
    /// Converte o lado do tabuleiro escolhido na jogada para o enum persistente.
    /// </summary>
    /// <param name="lado">O lado apurado pelo motor do jogo.</param>
    /// <returns>O lado correspondente no modelo persistente.</returns>
    private static LadoTabuleiro? MapearLado(LadoDoJogo? lado) =>
        lado switch
        {
            LadoDoJogo.Esquerda => LadoTabuleiro.Esquerda,
            LadoDoJogo.Direita => LadoTabuleiro.Direita,
            _ => null
        };

    /// <summary>
    /// Valida a pontuacao alvo informada para a partida.
    /// </summary>
    /// <param name="pontuacaoAlvo">A pontuacao que encerra a partida.</param>
    /// <exception cref="RegraDeNegocioException">Quando a pontuacao alvo nao for positiva.</exception>
    private static void ValidarPontuacaoAlvo(int pontuacaoAlvo)
    {
        if (pontuacaoAlvo < 1)
            throw new RegraDeNegocioException("A pontuacao alvo da partida deve ser maior que zero.");
    }
}
