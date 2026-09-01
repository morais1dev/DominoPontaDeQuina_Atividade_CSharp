using DominoPontaDeQuina.Core.Exceptions;

namespace DominoPontaDeQuina.Services.Exceptions;

/// <summary>
/// Representa a excecao lancada quando uma regra de uso da aplicacao e violada.
/// Essa situacao e esperada em cenarios como cadastrar um e-mail ja utilizado, informar credenciais
/// invalidas ou montar uma partida com uma formacao de jogadores incompativel.
/// </summary>
/// <param name="mensagem">A mensagem que descreve a regra de negocio violada.</param>
public class RegraDeNegocioException(string mensagem) : DominoException(mensagem)
{
}
