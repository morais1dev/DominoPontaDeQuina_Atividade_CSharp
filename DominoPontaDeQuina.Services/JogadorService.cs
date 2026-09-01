using DominoPontaDeQuina.Domain.Entities;
using DominoPontaDeQuina.Repository.Interfaces;
using DominoPontaDeQuina.Services.Exceptions;
using DominoPontaDeQuina.Services.Interfaces;

namespace DominoPontaDeQuina.Services;

/// <inheritdoc cref="IJogadorService"/>
public class JogadorService : IJogadorService
{
    /// <summary>
    /// Quantidade maxima de perfis de jogador que um usuario pode manter.
    /// O limite acompanha a quantidade de assentos de uma mesa em duplas.
    /// </summary>
    public const int MaximoDeJogadoresPorUsuario = 4;

    /// <summary>
    /// Quantidade minima de caracteres exigida para o nome de exibicao.
    /// </summary>
    private const int TamanhoMinimoDoNome = 3;

    private readonly IJogadorRepository _jogadorRepository;
    private readonly IUsuarioRepository _usuarioRepository;

    public JogadorService(IJogadorRepository jogadorRepository, IUsuarioRepository usuarioRepository)
    {
        _jogadorRepository = jogadorRepository;
        _usuarioRepository = usuarioRepository;
    }

    /// <inheritdoc />
    /// <exception cref="RecursoNaoEncontradoException">Quando o usuario informado nao existir.</exception>
    /// <exception cref="RegraDeNegocioException">Quando o nome for invalido, ja estiver em uso ou o limite de perfis for atingido.</exception>
    public async Task<Jogador> CriarAsync(Guid usuarioId, string nomeExibicao)
    {
        var nomeNormalizado = nomeExibicao?.Trim() ?? string.Empty;

        if (nomeNormalizado.Length < TamanhoMinimoDoNome)
            throw new RegraDeNegocioException($"O nome de exibicao deve possuir ao menos {TamanhoMinimoDoNome} caracteres.");

        if (await _usuarioRepository.ObterPorIdAsync(usuarioId) is null)
            throw RecursoNaoEncontradoException.Para("usuario", usuarioId);

        if (await _jogadorRepository.NomeExibicaoJaUsadoAsync(usuarioId, nomeNormalizado))
            throw new RegraDeNegocioException($"O usuario ja possui um jogador chamado {nomeNormalizado}.");

        if (await _jogadorRepository.ContarPorUsuarioAsync(usuarioId) >= MaximoDeJogadoresPorUsuario)
            throw new RegraDeNegocioException($"Um usuario pode manter no maximo {MaximoDeJogadoresPorUsuario} jogadores.");

        var jogador = new Jogador
        {
            UsuarioId = usuarioId,
            NomeExibicao = nomeNormalizado
        };

        return await _jogadorRepository.AdicionarAsync(jogador);
    }

    /// <inheritdoc />
    /// <exception cref="RecursoNaoEncontradoException">Quando o jogador nao existir.</exception>
    public async Task<Jogador> ObterPorIdAsync(Guid jogadorId)
    {
        return await _jogadorRepository.ObterPorIdAsync(jogadorId)
            ?? throw RecursoNaoEncontradoException.Para("jogador", jogadorId);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Jogador>> ListarPorUsuarioAsync(Guid usuarioId) =>
        await _jogadorRepository.ListarPorUsuarioAsync(usuarioId);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Jogador>> BuscarPorNomeExibicaoAsync(string trechoDoNome) =>
        await _jogadorRepository.BuscarPorNomeExibicaoAsync(trechoDoNome?.Trim() ?? string.Empty);

    /// <inheritdoc />
    /// <exception cref="RecursoNaoEncontradoException">Quando o jogador nao existir.</exception>
    public async Task RemoverAsync(Guid jogadorId)
    {
        var jogador = await ObterPorIdAsync(jogadorId);

        await _jogadorRepository.RemoverAsync(jogador);
    }
}
