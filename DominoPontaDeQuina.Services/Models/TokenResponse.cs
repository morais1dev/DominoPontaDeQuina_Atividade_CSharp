namespace DominoPontaDeQuina.Services.Models;

/// <summary>
/// Representa a resposta contendo o token JWT gerado apos autenticacao bem-sucedida.
/// </summary>
public class TokenResponse
{
    /// <summary>
    /// O token JWT no formato Bearer.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de token (normalmente "Bearer").
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Tempo de expiracao do token em segundos.
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Data e hora de expiracao do token em UTC.
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}
