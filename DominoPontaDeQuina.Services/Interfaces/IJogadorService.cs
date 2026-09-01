using DominoPontaDeQuina.Domain.Entities;

namespace DominoPontaDeQuina.Services.Interfaces;

/// <summary>
/// Define as regras de uso dos perfis de jogador vinculados a um usuario.
/// Um usuario representa a conta do aplicativo e cada jogador representa um perfil que pode ocupar
/// um assento na mesa de uma partida.
/// </summary>
public interface IJogadorService
{
    /// <summary>
    /// Cria um perfil de jogador para o usuario informado.
    /// </summary>
    /// <param name="usuarioId">O identificador do usuario dono do perfil.</param>
    /// <param name="nomeExibicao">O nome exibido nas partidas.</param>
    /// <returns>O jogador criado.</returns>
    Task<Jogador> CriarAsync(Guid usuarioId, string nomeExibicao);

    /// <summary>
    /// Obtem o jogador pelo identificador informado.
    /// </summary>
    /// <param name="jogadorId">O identificador do jogador.</param>
    /// <returns>O jogador encontrado.</returns>
    Task<Jogador> ObterPorIdAsync(Guid jogadorId);

    /// <summary>
    /// Lista os perfis de jogador do usuario informado.
    /// </summary>
    /// <param name="usuarioId">O identificador do usuario.</param>
    /// <returns>Os jogadores do usuario.</returns>
    Task<IReadOnlyList<Jogador>> ListarPorUsuarioAsync(Guid usuarioId);

    /// <summary>
    /// Busca jogadores cujo nome de exibicao contenha o trecho informado.
    /// </summary>
    /// <param name="trechoDoNome">O trecho de nome pesquisado.</param>
    /// <returns>Os jogadores que atendem ao filtro.</returns>
    Task<IReadOnlyList<Jogador>> BuscarPorNomeExibicaoAsync(string trechoDoNome);

    /// <summary>
    /// Remove o perfil de jogador informado.
    /// </summary>
    /// <param name="jogadorId">O identificador do jogador.</param>
    Task RemoverAsync(Guid jogadorId);
}
