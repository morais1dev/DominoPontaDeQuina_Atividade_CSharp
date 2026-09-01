using DominoPontaDeQuina.Domain.Entities;
using DominoPontaDeQuina.Repository.Context;
using DominoPontaDeQuina.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DominoPontaDeQuina.Repository.Repositories;

/// <inheritdoc cref="IJogadaRepository"/>
public class JogadaRepository : IJogadaRepository
{
    private readonly DominoDbContext _context;

    public JogadaRepository(DominoDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<Jogada> AdicionarAsync(Jogada jogada)
    {
        _context.Jogadas.Add(jogada);
        await _context.SaveChangesAsync();
        return jogada;
    }

    /// <inheritdoc />
    public async Task AdicionarVariasAsync(IEnumerable<Jogada> jogadas)
    {
        _context.Jogadas.AddRange(jogadas);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task AtualizarAsync(Jogada jogada)
    {
        _context.Jogadas.Update(jogada);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task RemoverAsync(Jogada jogada)
    {
        _context.Jogadas.Remove(jogada);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<Jogada?> ObterPorIdAsync(Guid id)
    {
        return await _context.Jogadas
            .FirstOrDefaultAsync(jogada => jogada.Id == id);
    }

    /// <inheritdoc />
    public async Task<List<Jogada>> ListarPorRodadaAsync(Guid rodadaId)
    {
        return await _context.Jogadas
            .Include(jogada => jogada.Jogador)
            .Where(jogada => jogada.RodadaId == rodadaId)
            .OrderBy(jogada => jogada.Sequencia)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<int> ContarPorJogadorAsync(Guid jogadorId)
    {
        return await _context.Jogadas
            .CountAsync(jogada => jogada.JogadorId == jogadorId);
    }

    /// <inheritdoc />
    public async Task<int> ContarPassesDoJogadorAsync(Guid jogadorId)
    {
        return await _context.Jogadas
            .CountAsync(jogada => jogada.JogadorId == jogadorId && jogada.PassouVez);
    }

    /// <inheritdoc />
    public async Task<int> SomarPontosDoJogadorAsync(Guid jogadorId)
    {
        return await _context.Jogadas
            .Where(jogada => jogada.JogadorId == jogadorId)
            .SumAsync(jogada => jogada.PontosGerados);
    }
}
