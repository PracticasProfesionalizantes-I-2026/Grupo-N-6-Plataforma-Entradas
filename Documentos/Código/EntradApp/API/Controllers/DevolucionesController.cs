using EntradApp.BusinessLogic.Interfaces;
using EntradApp.Shared.Common;
using EntradApp.Shared.DTOs.Devolucion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EntradApp.API.Controllers;

[ApiController]
[Route("api/v1/devoluciones")]

public class DevolucionesController : ControllerBase
{
    private readonly IDevolucionService _devolucionService;

    public DevolucionesController(IDevolucionService devolucionService) => _devolucionService = devolucionService;

    [HttpPost("{entradaId}")]
    [Authorize(Policy = "Usuario")]
    public async Task<ActionResult<DevolucionResponseDTO>> SolicitarDevolucion(Guid entradaId)
    {
        var usuarioId = Guid.Parse(User.FindFirst("sub")!.Value);
        var dto = new DevolucionCreateDTO { EntradaId = entradaId };
        var result = await _devolucionService.SolicitarDevolucionAsync(usuarioId, dto);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpGet]
    [Authorize(Policy = "Usuario")]
    public async Task<ActionResult<PagedResult<DevolucionResumenDTO>>> GetMisDevoluciones(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var usuarioId = Guid.Parse(User.FindFirst("sub")!.Value);
        var result = await _devolucionService.GetMisDevolucionesAsync(usuarioId, page, pageSize);
        return Ok(result);
    }

    [HttpPut("{devolucionId}/completar")]
    [Authorize(Policy = "SuperAdmin")]
    public async Task<ActionResult<DevolucionResponseDTO>> CompletarDevolucion(Guid devolucionId)
    {
        var result = await _devolucionService.CompletarDevolucionAsync(devolucionId);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }
}