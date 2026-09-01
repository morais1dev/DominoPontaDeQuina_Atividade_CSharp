namespace DominoPontaDeQuina.Services.Interfaces;

/// <summary>
/// Define o contrato de protecao das senhas dos usuarios.
/// A senha em texto puro nunca e persistida: apenas o resultado derivado retornado por este servico.
/// </summary>
public interface IHashSenhaService
{
    /// <summary>
    /// Gera o hash da senha informada.
    /// </summary>
    /// <param name="senha">A senha em texto puro.</param>
    /// <returns>O hash que deve ser persistido junto ao usuario.</returns>
    string GerarHash(string senha);

    /// <summary>
    /// Verifica se a senha informada corresponde ao hash armazenado.
    /// </summary>
    /// <param name="senha">A senha em texto puro.</param>
    /// <param name="hashArmazenado">O hash previamente persistido.</param>
    /// <returns><see langword="true"/> quando a senha conferir; caso contrario, <see langword="false"/>.</returns>
    bool Verificar(string senha, string hashArmazenado);
}
