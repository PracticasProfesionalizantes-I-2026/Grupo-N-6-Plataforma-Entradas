using EntradApp.DataAccess.Entities;
using EntradApp.DataAccess.Repositories.Interfaces;
using EntradApp.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace EntradApp.DataAccess.Repositories.Implementations;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly EntradAppDbContext _context;

    public UsuarioRepository(EntradAppDbContext context) => _context = context;

    public async Task<Usuario?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<Usuario?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<Usuario?> GetByDniAsync(string dni, CancellationToken ct = default)
        => await _context.Usuarios.FirstOrDefaultAsync(u => u.Dni == dni, ct);

    public async Task<Result<Usuario>> CreateAsync(Usuario usuario, CancellationToken ct = default)
    {
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync(ct);
        return Result.Success(usuario);
    }

    public async Task<Result> UpdateAsync(Usuario usuario, CancellationToken ct = default)
    {
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> SoftDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var usuario = await _context.Usuarios.FindAsync([id], ct);
        if (usuario == null) return Result.Failure("Usuario no encontrado");
        
        usuario.EsBorrado = true;
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}