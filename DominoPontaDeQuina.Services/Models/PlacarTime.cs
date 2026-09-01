namespace DominoPontaDeQuina.Services.Models;

/// <summary>
/// Representa o placar de um time ao final da partida.
/// </summary>
/// <param name="Nome">O nome do time na partida.</param>
/// <param name="Pontuacao">A pontuacao acumulada pelo time.</param>
/// <param name="Vencedor">Indica se o time venceu a partida.</param>
/// <param name="Jogadores">Os nomes de exibicao dos jogadores do time.</param>
public record PlacarTime(string Nome, int Pontuacao, bool Vencedor, IReadOnlyList<string> Jogadores);
