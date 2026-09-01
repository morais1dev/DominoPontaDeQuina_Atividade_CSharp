namespace DominoPontaDeQuina.Repository.Interfaces;

/// <summary>
/// Define as operacoes de persistencia comuns a todos os repositorios de entidades.
/// As consultas especificas de cada agregado ficam nas interfaces derivadas, mantendo as expressoes
/// LINQ concentradas na camada de repositorio.
/// </summary>
/// <typeparam name="TEntidade">O tipo da entidade persistida.</typeparam>
public interface IRepositorioBase<TEntidade>
    where TEntidade : class
{
    /// <summary>
    /// Adiciona a entidade informada e confirma a alteracao no banco de dados.
    /// </summary>
    /// <param name="entidade">A entidade a ser adicionada.</param>
    /// <returns>A entidade persistida.</returns>
    Task<TEntidade> AdicionarAsync(TEntidade entidade);

    /// <summary>
    /// Atualiza a entidade informada e confirma a alteracao no banco de dados.
    /// </summary>
    /// <param name="entidade">A entidade a ser atualizada.</param>
    Task AtualizarAsync(TEntidade entidade);

    /// <summary>
    /// Remove a entidade informada e confirma a alteracao no banco de dados.
    /// </summary>
    /// <param name="entidade">A entidade a ser removida.</param>
    Task RemoverAsync(TEntidade entidade);

    /// <summary>
    /// Obtem a entidade pelo seu identificador.
    /// </summary>
    /// <param name="id">O identificador da entidade.</param>
    /// <returns>A entidade encontrada, ou <see langword="null"/> quando nao existir.</returns>
    Task<TEntidade?> ObterPorIdAsync(Guid id);
}
