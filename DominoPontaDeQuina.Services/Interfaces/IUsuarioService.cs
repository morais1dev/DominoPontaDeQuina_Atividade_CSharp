using DominoPontaDeQuina.Domain.Entities;

namespace DominoPontaDeQuina.Services.Interfaces;

/// <summary>
/// Define as regras de uso da conta de usuario do aplicativo.
/// Este servico concentra as validacoes de cadastro e de autenticacao, deixando o acesso ao banco
/// de dados por conta do repositorio correspondente.
/// </summary>
public interface IUsuarioService
{
    /// <summary>
    /// Cadastra um novo usuario apos validar os dados informados e a unicidade do e-mail.
    /// </summary>
    /// <param name="nome">O nome do usuario.</param>
    /// <param name="email">O e-mail usado para autenticacao.</param>
    /// <param name="senha">A senha em texto puro, armazenada apenas na forma de hash.</param>
    /// <returns>O usuario cadastrado.</returns>
    Task<Usuario> CadastrarAsync(string nome, string email, string senha);

    /// <summary>
    /// Autentica o usuario a partir do e-mail e da senha informados.
    /// </summary>
    /// <param name="email">O e-mail cadastrado.</param>
    /// <param name="senha">A senha em texto puro.</param>
    /// <returns>O usuario autenticado.</returns>
    Task<Usuario> AutenticarAsync(string email, string senha);

    /// <summary>
    /// Obtem o usuario pelo identificador informado.
    /// </summary>
    /// <param name="usuarioId">O identificador do usuario.</param>
    /// <returns>O usuario encontrado.</returns>
    Task<Usuario> ObterPorIdAsync(Guid usuarioId);

    /// <summary>
    /// Lista todos os usuarios cadastrados.
    /// </summary>
    /// <returns>Os usuarios em ordem alfabetica de nome.</returns>
    Task<IReadOnlyList<Usuario>> ListarAsync();

    /// <summary>
    /// Busca usuarios cujo nome contenha o trecho informado.
    /// </summary>
    /// <param name="trechoDoNome">O trecho de nome pesquisado.</param>
    /// <returns>Os usuarios que atendem ao filtro.</returns>
    Task<IReadOnlyList<Usuario>> BuscarPorNomeAsync(string trechoDoNome);

    /// <summary>
    /// Altera a senha do usuario apos confirmar a senha atual.
    /// </summary>
    /// <param name="usuarioId">O identificador do usuario.</param>
    /// <param name="senhaAtual">A senha atualmente cadastrada.</param>
    /// <param name="novaSenha">A nova senha desejada.</param>
    Task AlterarSenhaAsync(Guid usuarioId, string senhaAtual, string novaSenha);
}
