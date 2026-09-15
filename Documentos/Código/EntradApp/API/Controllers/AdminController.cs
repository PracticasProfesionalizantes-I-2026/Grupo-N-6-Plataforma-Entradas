using EntradApp.BusinessLogic.Interfaces;
using EntradApp.Shared.DTOs.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EntradApp.API.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Policy = "SuperAdmin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService) => _adminService = adminService;

    [HttpGet("eventos/pendientes")]
    public async Task<ActionResult<AdminEventosPendientesResponseDTO>> GetEventosPendientes(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _adminService.GetEventosPendientesAsync(page, pageSize);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpPatch("eventos/{id}/estado")]
    public async Task<ActionResult> CambiarEstadoEvento(Guid id, [FromBody] EventoEstadoUpdateDTO dto)
    {
        var adminId = Guid.Parse(User.FindFirst("sub")!.Value);
        
        if (dto.NuevoEstado == EntradApp.Shared.Enums.EstadoEvento.Aprobado)
        {
            var result = await _adminService.AprobarEventoAsync(adminId, id, dto.Observaciones);
            if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        }
        else if (dto.NuevoEstado == EntradApp.Shared.Enums.EstadoEvento.Rechazado)
        {
            if (string.IsNullOrWhiteSpace(dto.Observaciones))
                return BadRequest(new { error = "El motivo de rechazo es obligatorio" });
            
            var result = await _adminService.RechazarEventoAsync(adminId, id, dto.Observaciones);
            if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        }
        else
        {
            return BadRequest(new { error = "Estado no válido. Use 'Aprobado' o 'Rechazado'" });
        }

        return NoContent();
    }
}