using EntradApp.BusinessLogic.Interfaces;
using EntradApp.Shared.DTOs.Devolucion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EntradApp.API.Controllers;

[ApiController]
[Route("api/v1/devoluciones")]
[Authorize(Policy = "Usuario")]
public class DevolucionesController : ControllerBase
{
    private readonly IDevolucionService _devolucionService;

    public DevolucionesController(IDevolucionService devolucionService) => _devolucionService = devolucionService;

    [HttpPost("{entradaId}")]
    public async Task<ActionResult<DevolucionResponseDTO>> SolicitarDevolucion(Guid entradaId)
    {
        var usuarioId = Guid.Parse(User.FindFirst("sub")!.Value);
        var dto = new DevolucionCreateDTO { EntradaId = entradaId };
        var result = await _devolucionService.SolicitarDevolucionAsync(usuarioId, dto);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<DevolucionResumenDTO>>> GetMisDevoluciones(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var usuarioId = Guid.Parse(User.FindFirst("sub")!.Value);
        var result = await _devolucionService.GetMisDevolucionesAsync(usuarioId, page, pageSize);
        return Ok(result);
    }
}