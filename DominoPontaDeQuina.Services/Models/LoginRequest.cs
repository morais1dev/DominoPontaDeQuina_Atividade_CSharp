namespace DominoPontaDeQuina.Services.Models;

/// <summary>
/// Representa a requisicao de login com credenciais do usuario.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// O email do usuario para autenticacao.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// A senha em texto puro do usuario.
    /// </summary>
    public string Senha { get; set; } = string.Empty;
}
