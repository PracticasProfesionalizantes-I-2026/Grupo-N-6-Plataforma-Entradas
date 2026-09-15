using EntradApp.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EntradApp.API.Controllers;

[ApiController]
[Route("api/v1/interno/precios")]
[Authorize(Policy = "SuperAdmin")] // Solo jobs internos / SuperAdmin
public class PrecioDinamicoController : ControllerBase
{
    private readonly IPrecioDinamicoService _precioService;

    public PrecioDinamicoController(IPrecioDinamicoService precioService) => _precioService = precioService;

    [HttpPost("evaluar")]
    public async Task<ActionResult> EvaluarIncremento()
    {
        var result = await _precioService.EvaluarYAplicarIncrementoAsync();
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return Ok(new { evaluado = true });
    }
}