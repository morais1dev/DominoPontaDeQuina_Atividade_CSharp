using DominoPontaDeQuina.Repository.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DominoPontaDeQuina.Migrations;

/// <summary>
/// Cria o <see cref="DominoDbContext"/> usado pelas ferramentas de linha de comando do Entity Framework Core.
/// Em tempo de execucao o contexto e resolvido pelo container de injecao de dependencia da aplicacao;
/// em tempo de design as opcoes sao montadas aqui, a partir do argumento de conexao ou da variavel de ambiente.
/// </summary>
public class DominoDbContextFactory : IDesignTimeDbContextFactory<DominoDbContext>
{
    /// <summary>
    /// Nome da variavel de ambiente consultada quando nenhuma conexao e informada por argumento.
    /// </summary>
    private const string VariavelDeAmbiente = "DOMINO_CONNECTION_STRING";

    /// <summary>
    /// Prefixo aceito para informar a conexao diretamente na linha de comando.
    /// </summary>
    private const string PrefixoDoArgumento = "--connection=";

    /// <inheritdoc />
    public DominoDbContext CreateDbContext(string[] args)
    {
        var opcoes = new DbContextOptionsBuilder<DominoDbContext>()
            .UseSqlite(ObterStringDeConexao(args))
            .Options;

        return new DominoDbContext(opcoes);
    }

    /// <summary>
    /// Obtem a string de conexao usada em tempo de design.
    /// </summary>
    /// <param name="args">Os argumentos repassados pelas ferramentas do Entity Framework Core.</param>
    /// <returns>A string de conexao aplicada ao contexto.</returns>
    private static string ObterStringDeConexao(string[] args)
    {
        var argumento = args?.FirstOrDefault(argumento => argumento.StartsWith(PrefixoDoArgumento, StringComparison.OrdinalIgnoreCase));

        if (argumento is not null)
            return argumento[PrefixoDoArgumento.Length..];

        var variavelDeAmbiente = Environment.GetEnvironmentVariable(VariavelDeAmbiente);

        return string.IsNullOrWhiteSpace(variavelDeAmbiente)
            ? DominoDbContext.ConnectionStringPadrao
            : variavelDeAmbiente;
    }
}
