using DominoPontaDeQuina.Domain.Entities;

namespace DominoPontaDeQuina.Repository.Interfaces;

/// <summary>
/// Define as consultas de persistencia disponiveis para o perfil de jogador vinculado a um usuario.
/// </summary>
public interface IJogadorRepository : IRepositorioBase<Jogador>
{
    /// <summary>
    /// Lista todos os jogadores cadastrados em ordem alfabetica de nome de exibicao.
    /// </summary>
    /// <returns>Os jogadores cadastrados.</returns>
    Task<List<Jogador>> ListarTodosAsync();

    /// <summary>
    /// Lista os jogadores pertencentes ao usuario informado.
    /// </summary>
    /// <param name="usuarioId">O identificador do usuario.</param>
    /// <returns>Os jogadores do usuario em ordem alfabetica.</returns>
    Task<List<Jogador>> ListarPorUsuarioAsync(Guid usuarioId);

    /// <summary>
    /// Lista os jogadores cujos identificadores estejam na colecao informada.
    /// </summary>
    /// <param name="ids">Os identificadores pesquisados.</param>
    /// <returns>Os jogadores encontrados.</returns>
    Task<List<Jogador>> ListarPorIdsAsync(IEnumerable<Guid> ids);

    /// <summary>
    /// Busca jogadores cujo nome de exibicao contenha o trecho informado.
    /// </summary>
    /// <param name="trechoDoNome">O trecho de nome pesquisado.</param>
    /// <returns>Os jogadores que atendem ao filtro.</returns>
    Task<List<Jogador>> BuscarPorNomeExibicaoAsync(string trechoDoNome);

    /// <summary>
    /// Conta quantos jogadores pertencem ao usuario informado.
    /// </summary>
    /// <param name="usuarioId">O identificador do usuario.</param>
    /// <returns>A quantidade de perfis de jogador do usuario.</returns>
    Task<int> ContarPorUsuarioAsync(Guid usuarioId);

    /// <summary>
    /// Verifica se o usuario ja possui um jogador com o nome de exibicao informado.
    /// </summary>
    /// <param name="usuarioId">O identificador do usuario.</param>
    /// <param name="nomeExibicao">O nome de exibicao pesquisado.</param>
    /// <returns><see langword="true"/> quando o nome ja estiver em uso pelo usuario; caso contrario, <see langword="false"/>.</returns>
    Task<bool> NomeExibicaoJaUsadoAsync(Guid usuarioId, string nomeExibicao);
}
