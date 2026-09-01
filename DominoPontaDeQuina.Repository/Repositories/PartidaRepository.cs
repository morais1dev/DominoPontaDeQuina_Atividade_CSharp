using DominoPontaDeQuina.Domain.Entities;
using DominoPontaDeQuina.Domain.Enums;
using DominoPontaDeQuina.Repository.Context;
using DominoPontaDeQuina.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DominoPontaDeQuina.Repository.Repositories;

/// <inheritdoc cref="IPartidaRepository"/>
public class PartidaRepository : IPartidaRepository
{
    private readonly DominoDbContext _context;

    public PartidaRepository(DominoDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<Partida> AdicionarAsync(Partida partida)
    {
        _context.Partidas.Add(partida);
        await _context.SaveChangesAsync();
        return partida;
    }

    /// <inheritdoc />
    public async Task AtualizarAsync(Partida partida)
    {
        _context.Partidas.Update(partida);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task RemoverAsync(Partida partida)
    {
        _context.Partidas.Remove(partida);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<Partida?> ObterPorIdAsync(Guid id)
    {
        return await _context.Partidas
            .Include(partida => partida.Participacoes)
                .ThenInclude(participacao => participacao.Jogador)
            .FirstOrDefaultAsync(partida => partida.Id == id);
    }

    /// <inheritdoc />
    public async Task<Partida?> ObterCompletaPorIdAsync(Guid id)
    {
        return await _context.Partidas
            .Include(partida => partida.Times)
            .Include(partida => partida.Participacoes)
                .ThenInclude(participacao => participacao.Jogador)
            .Include(partida => partida.Rodadas)
                .ThenInclude(rodada => rodada.Jogadas)
            .AsSplitQuery()
            .FirstOrDefaultAsync(partida => partida.Id == id);
    }

    /// <inheritdoc />
    public async Task<List<Partida>> ListarPorStatusAsync(StatusPartida status)
    {
        return await _context.Partidas
            .Where(partida => partida.Status == status)
            .OrderByDescending(partida => partida.IniciadoEm)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<List<Partida>> ListarPorJogadorAsync(Guid jogadorId)
    {
        return await _context.Partidas
            .Where(partida => partida.Participacoes.Any(participacao => participacao.JogadorId == jogadorId))
            .OrderByDescending(partida => partida.IniciadoEm)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<List<Partida>> ListarUltimasAsync(int quantidade)
    {
        return await _context.Partidas
            .OrderByDescending(partida => partida.IniciadoEm)
            .Take(quantidade)
            .ToListAsync();
    }
}
