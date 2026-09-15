using EntradApp.BusinessLogic.Interfaces;
using EntradApp.Shared.DTOs.Compra;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EntradApp.API.Controllers;

[ApiController]
[Route("api/v1/compras")]
[Authorize(Policy = "Usuario")]
public class ComprasController : ControllerBase
{
    private readonly ICompraService _compraService;

    public ComprasController(ICompraService compraService) => _compraService = compraService;

    [HttpPost]
    public async Task<ActionResult<CompraResponseDTO>> CrearCompra(CompraCreateDTO dto)
    {
        var usuarioId = Guid.Parse(User.FindFirst("sub")!.Value);
        var result = await _compraService.CrearCompraAsync(usuarioId, dto);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return CreatedAtAction(nameof(GetCompra), new { id = result.Value!.Id }, result.Value);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CompraResponseDTO>> GetCompra(Guid id)
    {
        var usuarioId = Guid.Parse(User.FindFirst("sub")!.Value);
        var result = await _compraService.GetByIdAsync(id);
        if (!result.IsSuccess) return NotFound(new { error = result.Error });
        if (result.Value!.UsuarioId != usuarioId) return Forbid();
        return Ok(result.Value);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<CompraResponseDTO>>> GetMisCompras(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var usuarioId = Guid.Parse(User.FindFirst("sub")!.Value);
        var result = await _compraService.GetByUsuarioAsync(usuarioId, page, pageSize);
        return Ok(result);
    }

    [HttpPatch("{id}/confirmar-pago")]
    public async Task<ActionResult<CompraResponseDTO>> ConfirmarPago(Guid id)
    {
        var usuarioId = Guid.Parse(User.FindFirst("sub")!.Value);
        var result = await _compraService.ConfirmarPagoAsync(usuarioId, id);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpPatch("{id}/cancelar")]
    public async Task<ActionResult> CancelarCompra(Guid id)
    {
        var usuarioId = Guid.Parse(User.FindFirst("sub")!.Value);
        var result = await _compraService.CancelarCompraAsync(usuarioId, id);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return NoContent();
    }
}