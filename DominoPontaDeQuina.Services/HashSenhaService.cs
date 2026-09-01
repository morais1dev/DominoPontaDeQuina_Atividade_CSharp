using DominoPontaDeQuina.Services.Interfaces;
using System.Security.Cryptography;

namespace DominoPontaDeQuina.Services;

/// <inheritdoc cref="IHashSenhaService"/>
public class HashSenhaService : IHashSenhaService
{
    /// <summary>
    /// Quantidade de iteracoes aplicadas na derivacao da chave.
    /// </summary>
    private const int Iteracoes = 100_000;

    /// <summary>
    /// Tamanho em bytes do sal gerado para cada senha.
    /// </summary>
    private const int TamanhoDoSal = 16;

    /// <summary>
    /// Tamanho em bytes da chave derivada a partir da senha.
    /// </summary>
    private const int TamanhoDaChave = 32;

    /// <summary>
    /// Separador das partes que compoem o hash persistido.
    /// </summary>
    private const char Separador = '.';

    /// <inheritdoc />
    public string GerarHash(string senha)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(senha);

        var sal = RandomNumberGenerator.GetBytes(TamanhoDoSal);
        var chave = DerivarChave(senha, sal);

        return string.Join(Separador, Iteracoes, Convert.ToBase64String(sal), Convert.ToBase64String(chave));
    }

    /// <inheritdoc />
    public bool Verificar(string senha, string hashArmazenado)
    {
        if (string.IsNullOrWhiteSpace(senha) || string.IsNullOrWhiteSpace(hashArmazenado))
            return false;

        var partes = hashArmazenado.Split(Separador);

        if (partes.Length != 3 || !int.TryParse(partes[0], out var iteracoes))
            return false;

        var sal = Convert.FromBase64String(partes[1]);
        var chaveEsperada = Convert.FromBase64String(partes[2]);
        var chaveInformada = DerivarChave(senha, sal, iteracoes, chaveEsperada.Length);

        return CryptographicOperations.FixedTimeEquals(chaveInformada, chaveEsperada);
    }

    /// <summary>
    /// Deriva a chave da senha aplicando PBKDF2 com SHA-256.
    /// </summary>
    /// <param name="senha">A senha em texto puro.</param>
    /// <param name="sal">O sal utilizado na derivacao.</param>
    /// <param name="iteracoes">A quantidade de iteracoes aplicadas.</param>
    /// <param name="tamanhoDaChave">O tamanho em bytes da chave derivada.</param>
    /// <returns>A chave derivada da senha.</returns>
    private static byte[] DerivarChave(string senha, byte[] sal, int iteracoes = Iteracoes, int tamanhoDaChave = TamanhoDaChave) =>
        Rfc2898DeriveBytes.Pbkdf2(senha, sal, iteracoes, HashAlgorithmName.SHA256, tamanhoDaChave);
}
