using DominoPontaDeQuina.Domain.Entities;
using DominoPontaDeQuina.Services.Interfaces;
using DominoPontaDeQuina.Services.Models;

namespace DominoPontaDeQuina.Services;

/// <summary>
/// Classe auxiliar que fornece metodos de conveniencia para autenticacao JWT.
/// Combina os servicos de usuario e autenticacao para fluxos completos de login.
/// </summary>
public class JwtAuthenticationHelper
{
    private readonly IUsuarioService _usuarioService;
    private readonly IAuthenticationService _authenticationService;

    public JwtAuthenticationHelper(
        IUsuarioService usuarioService,
        IAuthenticationService authenticationService)
    {
        _usuarioService = usuarioService;
        _authenticationService = authenticationService;
    }

    /// <summary>
    /// Realiza o login do usuario e gera um token JWT.
    /// </summary>
    /// <param name="email">O email do usuario.</param>
    /// <param name="senha">A senha do usuario.</param>
    /// <returns>Uma resposta contendo o token JWT gerado.</returns>
    /// <exception cref="Exception">Quando as credenciais forem invalidas.</exception>
    public async Task<(TokenResponse Token, Usuario Usuario)> LoginAsync(string email, string senha)
    {
        // Autenticar o usuario verificando credenciais
        var usuario = await _usuarioService.AutenticarAsync(email, senha);

        // Gerar o token JWT
        var token = _authenticationService.GerarToken(usuario.Id, usuario.Email, usuario.Nome);

        return (token, usuario);
    }

    /// <summary>
    /// Valida um token JWT e retorna o usuario correspondente.
    /// </summary>
    /// <param name="token">O token JWT a ser validado.</param>
    /// <returns>O usuario associado ao token.</returns>
    /// <exception cref="InvalidOperationException">Quando o token for invalido ou expirado.</exception>
    /// <exception cref="Exception">Quando o usuario nao for encontrado.</exception>
    public async Task<Usuario> ValidarTokenEObterUsuarioAsync(string token)
    {
        // Validar o token e extrair o usuario ID
        var usuarioId = _authenticationService.ValidarToken(token);

        // Obter os dados completos do usuario
        var usuario = await _usuarioService.ObterPorIdAsync(usuarioId);

        return usuario;
    }

    /// <summary>
    /// Realiza o login do usuario usando um objeto LoginRequest.
    /// </summary>
    /// <param name="loginRequest">Os dados de login (email e senha).</param>
    /// <returns>Uma resposta contendo o token JWT gerado.</returns>
    public async Task<TokenResponse> LoginComRequestAsync(LoginRequest loginRequest)
    {
        var (token, _) = await LoginAsync(loginRequest.Email, loginRequest.Senha);
        return token;
    }
}
