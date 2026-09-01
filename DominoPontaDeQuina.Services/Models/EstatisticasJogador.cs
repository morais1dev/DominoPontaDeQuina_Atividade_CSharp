namespace DominoPontaDeQuina.Services.Models;

/// <summary>
/// Representa o desempenho acumulado de um jogador nas partidas ja registradas.
/// </summary>
/// <param name="JogadorId">O identificador do jogador.</param>
/// <param name="NomeExibicao">O nome exibido do jogador.</param>
/// <param name="PartidasDisputadas">A quantidade de partidas em que o jogador participou.</param>
/// <param name="PartidasVencidas">A quantidade de partidas vencidas pelo time do jogador.</param>
/// <param name="RodadasVencidas">A quantidade de rodadas em que o jogador bateu ou levou a melhor no travamento.</param>
/// <param name="JogadasRealizadas">A quantidade de jogadas executadas pelo jogador.</param>
/// <param name="VezesQuePassou">A quantidade de vezes em que o jogador precisou passar a vez.</param>
/// <param name="PontuacaoTotal">A soma dos pontos obtidos pelo jogador em todas as partidas.</param>
public record EstatisticasJogador(
    Guid JogadorId,
    string NomeExibicao,
    int PartidasDisputadas,
    int PartidasVencidas,
    int RodadasVencidas,
    int JogadasRealizadas,
    int VezesQuePassou,
    int PontuacaoTotal)
{
    /// <summary>
    /// Obtem a proporcao de partidas vencidas em relacao as partidas disputadas.
    /// </summary>
    public double Aproveitamento =>
        PartidasDisputadas == 0 ? 0 : (double)PartidasVencidas / PartidasDisputadas;
}
