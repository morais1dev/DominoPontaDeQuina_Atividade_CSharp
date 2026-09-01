namespace DominoPontaDeQuina.Services.Models;

/// <summary>
/// Representa o resultado consolidado de uma partida ja executada e persistida.
/// </summary>
/// <param name="PartidaId">O identificador da partida.</param>
/// <param name="PontuacaoAlvo">A pontuacao que encerrou a partida.</param>
/// <param name="TotalDeRodadas">A quantidade de rodadas disputadas.</param>
/// <param name="TotalDeJogadas">A quantidade de jogadas registradas na partida.</param>
/// <param name="TimeVencedor">O nome do time vencedor.</param>
/// <param name="Placar">O placar de cada time da partida.</param>
public record ResumoPartida(
    Guid PartidaId,
    int PontuacaoAlvo,
    int TotalDeRodadas,
    int TotalDeJogadas,
    string TimeVencedor,
    IReadOnlyList<PlacarTime> Placar);
