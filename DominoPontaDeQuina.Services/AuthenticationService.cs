using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DominoPontaDeQuina.Services.Interfaces;
using DominoPontaDeQuina.Services.Models;
using Microsoft.IdentityModel.Tokens;

namespace DominoPontaDeQuina.Services;

/// <summary>
/// Implementacao do servico de autenticacao via JWT (JSON Web Token).
/// Responsavel pela geracao e validacao de tokens para usuarios autenticados.
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly string _chaveSecreta;
    private readonly int _tempoExpiracaoMinutos;
    private readonly string _emissor;
    private readonly string _audiencia;

    /// <summary>
    /// Inicializa uma nova instancia do servico de autenticacao.
    /// </summary>
    /// <param name="chaveSecreta">A chave secreta para assinar os tokens JWT (minimo 32 caracteres).</param>
    /// <param name="tempoExpiracaoMinutos">Tempo de expiracao do token em minutos (padrao: 60).</param>
    /// <param name="emissor">O emissor do token (padrao: "DominoPontaDeQuinaApp").</param>
    /// <param name="audiencia">A audiencia do token (padrao: "DominoPontaDeQuinaUsers").</param>
    public AuthenticationService(
        string chaveSecreta,
        int tempoExpiracaoMinutos = 60,
        string emissor = "DominoPontaDeQuinaApp",
        string audiencia = "DominoPontaDeQuinaUsers")
    {
        if (string.IsNullOrWhiteSpace(chaveSecreta))
            throw new ArgumentException("A chave secreta nao pode estar vazia.", nameof(chaveSecreta));

        if (chaveSecreta.Length < 32)
            throw new ArgumentException("A chave secreta deve ter no minimo 32 caracteres.", nameof(chaveSecreta));

        if (tempoExpiracaoMinutos <= 0)
            throw new ArgumentException("O tempo de expiracao deve ser maior que zero.", nameof(tempoExpiracaoMinutos));

        _chaveSecreta = chaveSecreta;
        _tempoExpiracaoMinutos = tempoExpiracaoMinutos;
        _emissor = emissor;
        _audiencia = audiencia;
    }

    /// <inheritdoc />
    public TokenResponse GerarToken(Guid usuarioId, string email, string nome)
    {
        var dataExpiracao = DateTime.UtcNow.AddMinutes(_tempoExpiracaoMinutos);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, nome),
            new Claim("UserId", usuarioId.ToString())
        };

        var chaveSeguranca = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_chaveSecreta));
        var credenciaisAssinatura = new SigningCredentials(chaveSeguranca, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _emissor,
            audience: _audiencia,
            claims: claims,
            expires: dataExpiracao,
            signingCredentials: credenciaisAssinatura);

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenString = tokenHandler.WriteToken(token);

        return new TokenResponse
        {
            Token = tokenString,
            TokenType = "Bearer",
            ExpiresIn = _tempoExpiracaoMinutos * 60, // em segundos
            ExpiresAt = dataExpiracao
        };
    }

    /// <inheritdoc />
    public Guid ValidarToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("O token nao pode estar vazio.");

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var chaveSeguranca = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_chaveSecreta));

            var parametrosValidacao = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = chaveSeguranca,
                ValidateIssuer = true,
                ValidIssuer = _emissor,
                ValidateAudience = true,
                ValidAudience = _audiencia,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero // Nao permitir margem de tempo
            };

            var principal = tokenHandler.ValidateToken(token, parametrosValidacao, out SecurityToken validatedToken);

            // Extrair o UserId do token
            var usuarioIdClaim = principal.FindFirst("UserId");
            if (usuarioIdClaim == null)
                throw new InvalidOperationException("O token nao contem o identificador do usuario.");

            if (!Guid.TryParse(usuarioIdClaim.Value, out var usuarioId))
                throw new InvalidOperationException("O identificador do usuario no token e invalido.");

            return usuarioId;
        }
        catch (SecurityTokenException ex)
        {
            throw new InvalidOperationException($"Falha ao validar o token: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao processar o token: {ex.Message}", ex);
        }
    }
}
