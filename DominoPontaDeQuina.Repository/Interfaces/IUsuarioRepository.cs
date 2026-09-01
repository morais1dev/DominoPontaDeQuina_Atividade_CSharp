using DominoPontaDeQuina.Domain.Entities;

namespace DominoPontaDeQuina.Repository.Interfaces;

/// <summary>
/// Define as consultas de persistencia disponiveis para a conta de usuario do aplicativo.
/// </summary>
public interface IUsuarioRepository : IRepositorioBase<Usuario>
{
    /// <summary>
    /// Obtem o usuario pelo endereco de e-mail cadastrado.
    /// </summary>
    /// <param name="email">O e-mail utilizado no cadastro.</param>
    /// <returns>O usuario encontrado, ou <see langword="null"/> quando nao existir.</returns>
    Task<Usuario?> ObterPorEmailAsync(string email);

    /// <summary>
    /// Verifica se o e-mail informado ja pertence a algum usuario.
    /// </summary>
    /// <param name="email">O e-mail a ser verificado.</param>
    /// <returns><see langword="true"/> quando o e-mail ja estiver cadastrado; caso contrario, <see langword="false"/>.</returns>
    Task<bool> EmailJaCadastradoAsync(string email);

    /// <summary>
    /// Lista todos os usuarios em ordem alfabetica de nome.
    /// </summary>
    /// <returns>Os usuarios cadastrados.</returns>
    Task<List<Usuario>> ListarAsync();

    /// <summary>
    /// Busca usuarios cujo nome contenha o trecho informado.
    /// </summary>
    /// <param name="trechoDoNome">O trecho de nome pesquisado.</param>
    /// <returns>Os usuarios que atendem ao filtro.</returns>
    Task<List<Usuario>> BuscarPorNomeAsync(string trechoDoNome);
}
