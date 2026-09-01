using DominoPontaDeQuina.Domain.Entities;
using DominoPontaDeQuina.Domain.Enums;
using DominoPontaDeQuina.Repository.Context;
using DominoPontaDeQuina.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DominoPontaDeQuina.Repository.Repositories;

/// <inheritdoc cref="IRodadaRepository"/>
public class RodadaRepository : IRodadaRepository
{
    private readonly DominoDbContext _context;

    public RodadaRepository(DominoDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<Rodada> AdicionarAsync(Rodada rodada)
    {
        _context.Rodadas.Add(rodada);
        await _context.SaveChangesAsync();
        return rodada;
    }

    /// <inheritdoc />
    public async Task AtualizarAsync(Rodada rodada)
    {
        _context.Rodadas.Update(rodada);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task RemoverAsync(Rodada rodada)
    {
        _context.Rodadas.Remove(rodada);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<Rodada?> ObterPorIdAsync(Guid id)
    {
        return await _context.Rodadas
            .Include(rodada => rodada.Jogadas)
            .FirstOrDefaultAsync(rodada => rodada.Id == id);
    }

    /// <inheritdoc />
    public async Task<List<Rodada>> ListarPorPartidaAsync(Guid partidaId)
    {
        return await _context.Rodadas
            .Where(rodada => rodada.PartidaId == partidaId)
            .OrderBy(rodada => rodada.Numero)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Rodada?> ObterUltimaDaPartidaAsync(Guid partidaId)
    {
        return await _context.Rodadas
            .Where(rodada => rodada.PartidaId == partidaId)
            .OrderByDescending(rodada => rodada.Numero)
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<int> ContarPorPartidaAsync(Guid partidaId)
    {
        return await _context.Rodadas
            .CountAsync(rodada => rodada.PartidaId == partidaId);
    }

    /// <inheritdoc />
    public async Task<int> ContarVitoriasDoJogadorAsync(Guid jogadorId)
    {
        return await _context.Rodadas
            .CountAsync(rodada => rodada.JogadorVencedorId == jogadorId);
    }

    /// <inheritdoc />
    public async Task<List<Rodada>> ListarPorTipoFinalizacaoAsync(TipoFinalizacaoRodada tipoFinalizacao)
    {
        return await _context.Rodadas
            .Where(rodada => rodada.TipoFinalizacao == tipoFinalizacao)
            .OrderByDescending(rodada => rodada.IniciadaEm)
            .ToListAsync();
    }
}
