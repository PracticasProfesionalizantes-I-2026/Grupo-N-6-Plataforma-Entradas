using EntradApp.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EntradApp.API.Controllers;

[ApiController]
[Route("api/v1/interno/validar")]
[Authorize(Policy = "Usuario")] // Solo para uso interno desde ComprasController
public class ValidacionesController : ControllerBase
{
    private readonly IValidacionCompraService _validacionService;

    public ValidacionesController(IValidacionCompraService validacionService) => _validacionService = validacionService;

    [HttpPost("disponibilidad")]
    public async Task<ActionResult> ValidarDisponibilidad([FromBody] ValidarDisponibilidadDTO dto)
    {
        var result = await _validacionService.ValidarDisponibilidadAsync(dto.SectorId, dto.Cantidad);
        if (!result.IsSuccess) return Conflict(new { error = result.Error });
        return Ok(new { disponible = true });
    }

    [HttpPost("limite")]
    public async Task<ActionResult> ValidarLimite([FromBody] ValidarLimiteDTO dto)
    {
        var result = await _validacionService.ValidarLimiteMaximoAsync(dto.UsuarioId, dto.EventoId, dto.Cantidad);
        if (!result.IsSuccess) return Conflict(new { error = result.Error });
        return Ok(new { dentroDeLimite = true });
    }

    [HttpPost("dnis")]
    public async Task<ActionResult> ValidarDnis([FromBody] ValidarDnisDTO dto)
    {
        var result = await _validacionService.ValidarDnisDuplicadosAsync(dto.EventoId, dto.Dnis);
        if (!result.IsSuccess) return Conflict(new { error = result.Error });
        return Ok(new { dnisValidos = true });
    }
}

public record ValidarDisponibilidadDTO(Guid SectorId, int Cantidad);
public record ValidarLimiteDTO(Guid UsuarioId, Guid EventoId, int Cantidad);
public record ValidarDnisDTO(Guid EventoId, List<string> Dnis);