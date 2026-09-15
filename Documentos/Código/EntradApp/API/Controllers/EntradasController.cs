using EntradApp.BusinessLogic.Interfaces;
using EntradApp.Shared.DTOs.Entrada;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EntradApp.API.Controllers;

[ApiController]
[Route("api/v1/entradas")]
[Authorize(Policy = "Usuario")]
public class EntradasController : ControllerBase
{
    private readonly IEntradaService _entradaService;

    public EntradasController(IEntradaService entradaService) => _entradaService = entradaService;

    [HttpGet]
    public async Task<ActionResult<PagedResult<EntradaResponseDTO>>> GetMisEntradas(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var usuarioId = Guid.Parse(User.FindFirst("sub")!.Value);
        var result = await _entradaService.GetMisEntradasAsync(usuarioId, page, pageSize);
        return Ok(result);
    }

    [HttpGet("utilizadas")]
    public async Task<ActionResult<PagedResult<EntradaResponseDTO>>> GetUtilizadas(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var usuarioId = Guid.Parse(User.FindFirst("sub")!.Value);
        var result = await _entradaService.GetUtilizadasAsync(usuarioId, page, pageSize);
        return Ok(result);
    }

    [HttpGet("devueltas")]
    public async Task<ActionResult<PagedResult<EntradaResponseDTO>>> GetDevueltas(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var usuarioId = Guid.Parse(User.FindFirst("sub")!.Value);
        var result = await _entradaService.GetDevueltasAsync(usuarioId, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}/comprobante")]
    public async Task<ActionResult> GetComprobante(Guid id)
    {
        var usuarioId = Guid.Parse(User.FindFirst("sub")!.Value);
        try
        {
            var pdf = await _entradaService.GenerarComprobanteAsync(usuarioId, id);
            return File(pdf, "application/pdf", $"comprobante-{id}.pdf");
        }
        catch (Exception ex) when (ex is KeyNotFoundException || ex is UnauthorizedAccessException)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}