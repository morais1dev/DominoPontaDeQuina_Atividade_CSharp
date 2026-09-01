using DominoPontaDeQuina.Domain.Entities;
using DominoPontaDeQuina.Repository.Context;
using DominoPontaDeQuina.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DominoPontaDeQuina.Repository.Repositories;

/// <inheritdoc cref="IJogadorRepository"/>
public class JogadorRepository : IJogadorRepository
{
    private readonly DominoDbContext _context;

    public JogadorRepository(DominoDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<Jogador> AdicionarAsync(Jogador jogador)
    {
        _context.Jogadores.Add(jogador);
        await _context.SaveChangesAsync();
        return jogador;
    }

    /// <inheritdoc />
    public async Task AtualizarAsync(Jogador jogador)
    {
        _context.Jogadores.Update(jogador);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task RemoverAsync(Jogador jogador)
    {
        _context.Jogadores.Remove(jogador);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<Jogador?> ObterPorIdAsync(Guid id)
    {
        return await _context.Jogadores
            .Include(jogador => jogador.Usuario)
            .FirstOrDefaultAsync(jogador => jogador.Id == id);
    }

    /// <inheritdoc />
    public async Task<List<Jogador>> ListarTodosAsync()
    {
        return await _context.Jogadores
            .OrderBy(jogador => jogador.NomeExibicao)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<List<Jogador>> ListarPorUsuarioAsync(Guid usuarioId)
    {
        return await _context.Jogadores
            .Where(jogador => jogador.UsuarioId == usuarioId)
            .OrderBy(jogador => jogador.NomeExibicao)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<List<Jogador>> ListarPorIdsAsync(IEnumerable<Guid> ids)
    {
        var identificadores = ids.Distinct().ToList();

        return await _context.Jogadores
            .Where(jogador => identificadores.Contains(jogador.Id))
            .OrderBy(jogador => jogador.NomeExibicao)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<List<Jogador>> BuscarPorNomeExibicaoAsync(string trechoDoNome)
    {
        return await _context.Jogadores
            .Where(jogador => jogador.NomeExibicao.Contains(trechoDoNome))
            .OrderBy(jogador => jogador.NomeExibicao)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<int> ContarPorUsuarioAsync(Guid usuarioId)
    {
        return await _context.Jogadores
            .CountAsync(jogador => jogador.UsuarioId == usuarioId);
    }

    /// <inheritdoc />
    public async Task<bool> NomeExibicaoJaUsadoAsync(Guid usuarioId, string nomeExibicao)
    {
        return await _context.Jogadores
            .AnyAsync(jogador => jogador.UsuarioId == usuarioId && jogador.NomeExibicao == nomeExibicao);
    }
}
