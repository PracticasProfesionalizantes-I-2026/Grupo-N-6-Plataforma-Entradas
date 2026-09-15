using EntradApp.DataAccess.Entities;
using EntradApp.Shared.Common;

namespace EntradApp.DataAccess.Repositories.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Usuario?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<Usuario?> GetByDniAsync(string dni, CancellationToken ct = default);
    Task<Result<Usuario>> CreateAsync(Usuario usuario, CancellationToken ct = default);
    Task<Result> UpdateAsync(Usuario usuario, CancellationToken ct = default);
    Task<Result> SoftDeleteAsync(Guid id, CancellationToken ct = default);
}