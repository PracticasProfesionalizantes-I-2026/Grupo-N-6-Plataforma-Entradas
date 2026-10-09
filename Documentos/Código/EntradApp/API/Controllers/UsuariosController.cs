using EntradApp.BusinessLogic.Interfaces;
using EntradApp.Shared.DTOs.Usuario;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EntradApp.API.Controllers;

[ApiController]
[Route("api/v1/usuarios")]
[Authorize]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [HttpGet("perfil")]
    public async Task<ActionResult<UsuarioResponseDTO>> GetPerfil()
    {
        var usuarioId = ObtenerUsuarioId();
        var result = await _usuarioService.GetByIdAsync(usuarioId);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpPut("perfil")]
    public async Task<ActionResult> ActualizarPerfil([FromBody] UsuarioUpdateDTO dto)
    {
        var usuarioId = ObtenerUsuarioId();
        var result = await _usuarioService.ActualizarDatosPersonalesAsync(usuarioId, dto);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return NoContent();
    }

    [HttpPut("password")]
    public async Task<ActionResult> ActualizarPassword([FromBody] ActualizarPasswordDTO dto)
    {
        var usuarioId = ObtenerUsuarioId();
        var result = await _usuarioService.ActualizarPasswordAsync(usuarioId, dto.PasswordActual, dto.PasswordNueva);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return NoContent();
    }

    private Guid ObtenerUsuarioId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(claim) || !Guid.TryParse(claim, out var usuarioId))
            throw new UnauthorizedAccessException("Identificador de usuario inválido o no presente en el token");
        return usuarioId;
    }
}
