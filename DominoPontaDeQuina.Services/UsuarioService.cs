using DominoPontaDeQuina.Domain.Entities;
using DominoPontaDeQuina.Repository.Interfaces;
using DominoPontaDeQuina.Services.Exceptions;
using DominoPontaDeQuina.Services.Interfaces;

namespace DominoPontaDeQuina.Services;

/// <inheritdoc cref="IUsuarioService"/>
public class UsuarioService : IUsuarioService
{
    /// <summary>
    /// Quantidade minima de caracteres exigida para a senha do usuario.
    /// </summary>
    private const int TamanhoMinimoDaSenha = 6;

    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IHashSenhaService _hashSenhaService;

    public UsuarioService(IUsuarioRepository usuarioRepository, IHashSenhaService hashSenhaService)
    {
        _usuarioRepository = usuarioRepository;
        _hashSenhaService = hashSenhaService;
    }

    /// <inheritdoc />
    /// <exception cref="RegraDeNegocioException">Quando os dados forem invalidos ou o e-mail ja estiver cadastrado.</exception>
    public async Task<Usuario> CadastrarAsync(string nome, string email, string senha)
    {
        var nomeNormalizado = NormalizarTexto(nome);
        var emailNormalizado = NormalizarEmail(email);

        if (nomeNormalizado.Length == 0)
            throw new RegraDeNegocioException("O nome do usuario e obrigatorio.");

        ValidarEmail(emailNormalizado);
        ValidarSenha(senha);

        if (await _usuarioRepository.EmailJaCadastradoAsync(emailNormalizado))
            throw new RegraDeNegocioException($"O e-mail {emailNormalizado} ja esta cadastrado.");

        var usuario = new Usuario
        {
            Nome = nomeNormalizado,
            Email = emailNormalizado,
            HashSenha = _hashSenhaService.GerarHash(senha)
        };

        return await _usuarioRepository.AdicionarAsync(usuario);
    }

    /// <inheritdoc />
    /// <exception cref="RegraDeNegocioException">Quando as credenciais informadas nao forem validas.</exception>
    public async Task<Usuario> AutenticarAsync(string email, string senha)
    {
        var usuario = await _usuarioRepository.ObterPorEmailAsync(NormalizarEmail(email));

        if (usuario is null || !_hashSenhaService.Verificar(senha ?? string.Empty, usuario.HashSenha))
            throw new RegraDeNegocioException("E-mail ou senha invalidos.");

        return usuario;
    }

    /// <inheritdoc />
    /// <exception cref="RecursoNaoEncontradoException">Quando o usuario nao existir.</exception>
    public async Task<Usuario> ObterPorIdAsync(Guid usuarioId)
    {
        return await _usuarioRepository.ObterPorIdAsync(usuarioId)
            ?? throw RecursoNaoEncontradoException.Para("usuario", usuarioId);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Usuario>> ListarAsync() =>
        await _usuarioRepository.ListarAsync();

    /// <inheritdoc />
    public async Task<IReadOnlyList<Usuario>> BuscarPorNomeAsync(string trechoDoNome) =>
        await _usuarioRepository.BuscarPorNomeAsync(NormalizarTexto(trechoDoNome));

    /// <inheritdoc />
    /// <exception cref="RecursoNaoEncontradoException">Quando o usuario nao existir.</exception>
    /// <exception cref="RegraDeNegocioException">Quando a senha atual nao conferir ou a nova senha for invalida.</exception>
    public async Task AlterarSenhaAsync(Guid usuarioId, string senhaAtual, string novaSenha)
    {
        var usuario = await ObterPorIdAsync(usuarioId);

        if (!_hashSenhaService.Verificar(senhaAtual ?? string.Empty, usuario.HashSenha))
            throw new RegraDeNegocioException("A senha atual informada nao confere.");

        ValidarSenha(novaSenha);

        usuario.HashSenha = _hashSenhaService.GerarHash(novaSenha);

        await _usuarioRepository.AtualizarAsync(usuario);
    }

    /// <summary>
    /// Valida o formato do e-mail informado.
    /// </summary>
    /// <param name="email">O e-mail ja normalizado.</param>
    /// <exception cref="RegraDeNegocioException">Quando o e-mail estiver ausente ou mal formado.</exception>
    private static void ValidarEmail(string email)
    {
        var posicaoDoArroba = email.IndexOf('@');

        if (posicaoDoArroba <= 0 || posicaoDoArroba == email.Length - 1 || email.Contains(' '))
            throw new RegraDeNegocioException($"O e-mail {email} nao possui um formato valido.");
    }

    /// <summary>
    /// Valida o tamanho minimo da senha informada.
    /// </summary>
    /// <param name="senha">A senha em texto puro.</param>
    /// <exception cref="RegraDeNegocioException">Quando a senha for menor que o tamanho minimo exigido.</exception>
    private static void ValidarSenha(string senha)
    {
        if (string.IsNullOrWhiteSpace(senha) || senha.Length < TamanhoMinimoDaSenha)
            throw new RegraDeNegocioException($"A senha deve possuir ao menos {TamanhoMinimoDaSenha} caracteres.");
    }

    /// <summary>
    /// Remove espacos das extremidades do texto informado.
    /// </summary>
    /// <param name="texto">O texto original.</param>
    /// <returns>O texto sem espacos nas extremidades.</returns>
    private static string NormalizarTexto(string texto) =>
        texto?.Trim() ?? string.Empty;

    /// <summary>
    /// Normaliza o e-mail para comparacao, removendo espacos e aplicando caixa baixa.
    /// </summary>
    /// <param name="email">O e-mail original.</param>
    /// <returns>O e-mail normalizado.</returns>
    private static string NormalizarEmail(string email) =>
        NormalizarTexto(email).ToLowerInvariant();
}
