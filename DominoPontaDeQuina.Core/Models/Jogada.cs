using DominoPontaDeQuina.Core.Enums;
using DominoPontaDeQuina.Core.Interfaces;

namespace DominoPontaDeQuina.Core.Models;

/// <inheritdoc cref="IJogada"/>
/// <param name="jogador">O jogador da jogada.</param>
/// <param name="peca">A peca jogada, quando houver.</param>
/// <param name="valorColado">O valor colado na jogada, quando houver.</param>
/// <param name="lado">O lado escolhido, quando houver.</param>
public class Jogada(Jogador jogador, Peca? peca = null, int? valorColado = null, LadoTabuleiro? lado = null) : IJogada
{
    /// <inheritdoc />
    public Jogador Jogador { get; } = jogador;

    /// <inheritdoc />
    public Peca? Peca { get; } = peca;

    /// <summary>
    /// Obtem o valor da ponta em que a peca foi encaixada.
    /// Quando o tabuleiro estava vazio ou a jogada passou a vez, nao existe valor colado.
    /// </summary>
    public int? ValorColado { get; } = valorColado;

    /// <inheritdoc />
    public LadoTabuleiro? Lado { get; } = lado;

    /// <inheritdoc />
    public StatusJogada Status { get; private set; } = StatusJogada.Pendente;

    /// <summary>
    /// Obtem a pontuacao gerada por esta jogada.
    /// A jogada pontua quando a soma das pontas externas do tabuleiro e multipla de cinco.
    /// </summary>
    public int PontosGerados { get; private set; }

    /// <inheritdoc />
    public bool EhPassarVez() =>
        Peca is null && Lado is null;

    /// <inheritdoc />
    public void MarcarComoAplicada() =>
        Status = StatusJogada.Aplicada;

    /// <inheritdoc />
    public void MarcarComoInvalida() =>
        Status = StatusJogada.Invalida;

    /// <summary>
    /// Registra a pontuacao apurada para esta jogada pela regra das pontas externas.
    /// </summary>
    /// <param name="pontos">Os pontos gerados pela jogada.</param>
    public void RegistrarPontuacao(int pontos) =>
        PontosGerados = pontos;
}
