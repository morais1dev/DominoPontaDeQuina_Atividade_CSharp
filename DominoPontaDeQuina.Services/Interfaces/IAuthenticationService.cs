using DominoPontaDeQuina.Services.Models;

namespace DominoPontaDeQuina.Services.Interfaces;

/// <summary>
/// Define as regras de autenticacao via JWT (JSON Web Token).
/// Este servico concentra a geracao e validacao de tokens JWT.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Gera um token JWT para um usuario autenticado.
    /// </summary>
    /// <param name="usuarioId">O identificador do usuario.</param>
    /// <param name="email">O email do usuario.</param>
    /// <param name="nome">O nome do usuario.</param>
    /// <returns>Um objeto contendo o token JWT e sua data de expiracao.</returns>
    TokenResponse GerarToken(Guid usuarioId, string email, string nome);

    /// <summary>
    /// Valida um token JWT fornecido.
    /// </summary>
    /// <param name="token">O token JWT a ser validado.</param>
    /// <returns>O usuario ID contido no token, se valido.</returns>
    /// <exception cref="InvalidOperationException">Quando o token e invalido ou expirado.</exception>
    Guid ValidarToken(string token);
}
