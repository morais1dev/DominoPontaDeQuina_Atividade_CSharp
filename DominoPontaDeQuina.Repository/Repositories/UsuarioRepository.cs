using DominoPontaDeQuina.Domain.Entities;
using DominoPontaDeQuina.Repository.Context;
using DominoPontaDeQuina.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DominoPontaDeQuina.Repository.Repositories;

/// <inheritdoc cref="IUsuarioRepository"/>
public class UsuarioRepository : IUsuarioRepository
{
    private readonly DominoDbContext _context;

    public UsuarioRepository(DominoDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<Usuario> AdicionarAsync(Usuario usuario)
    {
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
        return usuario;
    }

    /// <inheritdoc />
    public async Task AtualizarAsync(Usuario usuario)
    {
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task RemoverAsync(Usuario usuario)
    {
        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<Usuario?> ObterPorIdAsync(Guid id)
    {
        return await _context.Usuarios
            .Include(usuario => usuario.Jogadores)
            .FirstOrDefaultAsync(usuario => usuario.Id == id);
    }

    /// <inheritdoc />
    public async Task<Usuario?> ObterPorEmailAsync(string email)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(usuario => usuario.Email == email);
    }

    /// <inheritdoc />
    public async Task<bool> EmailJaCadastradoAsync(string email)
    {
        return await _context.Usuarios
            .AnyAsync(usuario => usuario.Email == email);
    }

    /// <inheritdoc />
    public async Task<List<Usuario>> ListarAsync()
    {
        return await _context.Usuarios
            .OrderBy(usuario => usuario.Nome)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<List<Usuario>> BuscarPorNomeAsync(string trechoDoNome)
    {
        return await _context.Usuarios
            .Where(usuario => usuario.Nome.Contains(trechoDoNome))
            .OrderBy(usuario => usuario.Nome)
            .ToListAsync();
    }
}
