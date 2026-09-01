using DominoPontaDeQuina.Core.Models;
using System.Collections.ObjectModel;

namespace DominoPontaDeQuina.Core.Interfaces;

/// <summary>
/// Define o contrato de orquestracao no topo da hierarquia Partida -> Rodadas -> Jogadas.
/// Este contrato permite que camadas externas conduzam a partida rodada a rodada, sem conhecer
/// os detalhes de distribuicao de pecas, controle de turno ou apuracao de pontuacao.
/// </summary>
public interface IJogo
{
    /// <summary>
    /// Obtem o historico das partidas controladas por esta instancia.
    /// </summary>
    ReadOnlyCollection<Partida> HistoricoPartidas { get; }

    /// <summary>
    /// Obtem a partida atual controlada pelo jogo.
    /// </summary>
    Partida? PartidaAtual { get; }

    /// <summary>
    /// Inicia uma partida com os jogadores informados e deixa a primeira rodada pronta para ser executada.
    /// Os jogadores sao distribuidos alternadamente entre os dois times da partida.
    /// </summary>
    /// <param name="jogadores">Os jogadores participantes, em ordem de assento na mesa.</param>
    /// <param name="pontuacaoAlvo">A pontuacao que encerra a partida.</param>
    Task IniciarPartidaAsync(IReadOnlyList<Jogador> jogadores, int pontuacaoAlvo = Jogo.PontuacaoAlvoPadrao);

    /// <summary>
    /// Executa a rodada atual ate que ela seja encerrada por batida ou por travamento do tabuleiro.
    /// A pontuacao apurada na rodada e creditada aos times dos respectivos jogadores.
    /// </summary>
    /// <returns>A rodada ja finalizada, com seu historico de jogadas e pontuacao.</returns>
    Task<Rodada> ExecutarRodadaAtualAsync();

    /// <summary>
    /// Avanca a partida apos o encerramento de uma rodada.
    /// Quando a pontuacao alvo ja tiver sido atingida a partida e finalizada; caso contrario, uma nova rodada e iniciada.
    /// </summary>
    /// <returns><see langword="true"/> quando uma nova rodada foi iniciada; <see langword="false"/> quando a partida foi finalizada.</returns>
    bool AvancarParaProximaRodada();
}
