using EntradApp.BusinessLogic.Interfaces;
using EntradApp.Shared.DTOs.Usuario;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EntradApp.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public AuthController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UsuarioResponseDTO>> Register([FromBody] UsuarioCreateDTO dto)
    {
        var result = await _usuarioService.RegistrarAsync(dto);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(Register), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDTO>> Login([FromBody] LoginRequestDTO dto)
    {
        var result = await _usuarioService.LoginAsync(dto.Email, dto.Password);
        if (!result.IsSuccess)
            return Unauthorized(new { error = result.Error });

        return Ok(result.Value);
    }
}
