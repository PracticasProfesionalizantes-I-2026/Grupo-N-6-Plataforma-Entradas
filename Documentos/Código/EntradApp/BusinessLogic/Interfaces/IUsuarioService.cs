using EntradApp.DataAccess.Entities;
using EntradApp.Shared.Common;
using EntradApp.Shared.DTOs.Usuario;

namespace EntradApp.BusinessLogic.Interfaces;

public interface IUsuarioService
{
    Task<Result<UsuarioResponseDTO>> RegistrarAsync(UsuarioCreateDTO dto);
    Task<Result<UsuarioResponseDTO>> GetByIdAsync(Guid id);
    Task<Result<UsuarioResponseDTO>> GetByEmailAsync(string email);
    Task<Result> ActualizarDatosPersonalesAsync(Guid id, UsuarioUpdateDTO dto);
    Task<Result> ActualizarPasswordAsync(Guid id, string passwordActual, string passwordNueva);
    Task<Result> LoginAsync(string email, string password);
}