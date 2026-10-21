using EntradApp.BusinessLogic.Interfaces;
using EntradApp.Shared.DTOs.Entrada;
using EntradApp.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace EntradApp.API.Controllers;

[ApiController]
[Route("api/v1/control-acceso")]
[Authorize(Policy = "SuperAdmin")]
public class ControlAccesoController : ControllerBase
{
    private readonly IEntradaService _entradaService;
    private static readonly Regex CodigoUnicoRegex = new(@"^EVT-[A-F0-9]{8}$", RegexOptions.Compiled);

    public ControlAccesoController(IEntradaService entradaService) => _entradaService = entradaService;

    [HttpPost("validar")]
    public async Task<ActionResult<ValidarControlAccesoResponseDTO>> Validar([FromBody] ValidarControlAccesoRequestDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CodigoUnico))
            return BadRequest(new { error = "El código único es requerido" });

        if (!CodigoUnicoRegex.IsMatch(dto.CodigoUnico.Trim().ToUpper()))
            return BadRequest(new { error = "Formato de código único inválido. Debe ser como: EVT-A1B2C3D4" });

        var result = await _entradaService.ValidarEntradaAsync(dto.CodigoUnico.Trim().ToUpper());

        if (!result.IsSuccess)
        {
            return Ok(new ValidarControlAccesoResponseDTO
            {
                Valido = false,
                Error = result.Error,
                CodigoUnico = dto.CodigoUnico
            });
        }

        var entrada = result.Value!;
        return Ok(new ValidarControlAccesoResponseDTO
        {
            Valido = true,
            EntradaId = entrada.Id,
            CodigoUnico = entrada.CodigoUnico,
            Dni = entrada.Dni,
            SectorNombre = entrada.SectorNombre,
            EventoNombre = entrada.EventoNombre,
            EventoFechaInicio = entrada.EventoFechaInicio,
            EventoLugar = entrada.EventoLugar,
            EstadoAnterior = EstadoEntrada.Activa,
            FechaValidacion = entrada.FechaValidacion
        });
    }
}