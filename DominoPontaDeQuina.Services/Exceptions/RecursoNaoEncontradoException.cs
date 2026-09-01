using DominoPontaDeQuina.Core.Exceptions;

namespace DominoPontaDeQuina.Services.Exceptions;

/// <summary>
/// Representa a excecao lancada quando um registro exigido pelo fluxo nao existe na base de dados.
/// Essa situacao e esperada ao referenciar um usuario, um jogador ou uma partida por um identificador invalido.
/// </summary>
/// <param name="mensagem">A mensagem que descreve o recurso nao encontrado.</param>
public class RecursoNaoEncontradoException(string mensagem) : DominoException(mensagem)
{
    /// <summary>
    /// Cria a excecao para o caso em que um registro nao foi localizado pelo seu identificador.
    /// </summary>
    /// <param name="recurso">O nome do recurso pesquisado.</param>
    /// <param name="id">O identificador utilizado na busca.</param>
    /// <returns>A excecao com a mensagem detalhando o recurso ausente.</returns>
    public static RecursoNaoEncontradoException Para(string recurso, Guid id) =>
        new($"O recurso {recurso} com identificador {id} nao foi encontrado.");
}
