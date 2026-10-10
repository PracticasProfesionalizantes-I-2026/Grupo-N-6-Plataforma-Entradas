using EntradApp.BusinessLogic.Interfaces;
using EntradApp.DataAccess.Entities;
using EntradApp.DataAccess.Repositories.Interfaces;
using EntradApp.Shared.Common;
using EntradApp.Shared.DTOs.Compra;
using EntradApp.Shared.Enums;
using EntradApp.Shared.Exceptions;

namespace EntradApp.BusinessLogic.Services;

public class CompraService : ICompraService
{
    private readonly ICompraRepository _compraRepository;
    private readonly IEntradaRepository _entradaRepository;
    private readonly ISectorRepository _sectorRepository;
    private readonly IEventoRepository _eventoRepository;
    private readonly IValidacionCompraService _validacionCompraService;
    private readonly IReservaTemporalService _reservaTemporalService;

    public CompraService(
        ICompraRepository compraRepository,
        IEntradaRepository entradaRepository,
        ISectorRepository sectorRepository,
        IEventoRepository eventoRepository,
        IValidacionCompraService validacionCompraService,
        IReservaTemporalService reservaTemporalService)
    {
        _compraRepository = compraRepository;
        _entradaRepository = entradaRepository;
        _sectorRepository = sectorRepository;
        _eventoRepository = eventoRepository;
        _validacionCompraService = validacionCompraService;
        _reservaTemporalService = reservaTemporalService;
    }

    public async Task<Result<CompraResponseDTO>> CrearCompraAsync(Guid usuarioId, CompraCreateDTO dto)
    {
        var evento = await _eventoRepository.GetByIdAsync(dto.EventoId);
        if (evento == null) return Result.Failure<CompraResponseDTO>("Evento no encontrado");
        if (evento.Estado != EstadoEvento.Aprobado) return Result.Failure<CompraResponseDTO>("El evento no está disponible para compra");

        var sector = await _sectorRepository.GetByIdAsync(dto.SectorId);
        if (sector == null || sector.EventoId != dto.EventoId) return Result.Failure<CompraResponseDTO>("Sector no disponible");
        if (sector.EsBorrado) return Result.Failure<CompraResponseDTO>("Sector no disponible");

        if (dto.Dnis.Count != dto.Cantidad)
            return Result.Failure<CompraResponseDTO>("Debe proporcionar un DNI por cada entrada");

        var dispResult = await _validacionCompraService.ValidarDisponibilidadAsync(dto.SectorId, dto.Cantidad);
        if (!dispResult.IsSuccess)
            return Result.Failure<CompraResponseDTO>(dispResult.Error!);

        var limiteResult = await _validacionCompraService.ValidarLimiteMaximoAsync(usuarioId, dto.EventoId, dto.Cantidad);
        if (!limiteResult.IsSuccess)
            return Result.Failure<CompraResponseDTO>(limiteResult.Error!);

        var dnisResult = await _validacionCompraService.ValidarDnisDuplicadosAsync(dto.EventoId, dto.Dnis);
        if (!dnisResult.IsSuccess)
            return Result.Failure<CompraResponseDTO>(dnisResult.Error!);

        var reservaResult = await _reservaTemporalService.CrearReservaAsync(
            Guid.NewGuid(), dto.SectorId, dto.Cantidad, 10);
        if (!reservaResult.IsSuccess)
            return Result.Failure<CompraResponseDTO>(reservaResult.Error!);

        var compra = new Compra
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            EventoId = dto.EventoId,
            SectorId = dto.SectorId,
            Cantidad = dto.Cantidad,
            Total = sector.PrecioActual * dto.Cantidad,
            Estado = EstadoCompra.PendientePago,
            FechaCreacion = DateTime.UtcNow
        };

        await _compraRepository.CreateAsync(compra);

        var entradas = dto.Dnis.Select((dni, index) => new Entrada
        {
            Id = Guid.NewGuid(),
            CompraId = compra.Id,
            SectorId = dto.SectorId,
            Dni = dni.Trim(),
            CodigoUnico = GenerarCodigoUnico(),
            Estado = EstadoEntrada.Activa,
            FechaCreacion = DateTime.UtcNow
        }).ToList();

        await _entradaRepository.CreateRangeAsync(entradas);
        await _sectorRepository.IncrementarVendidasAsync(dto.SectorId, dto.Cantidad);
        await _sectorRepository.DecrementarReservadasAsync(dto.SectorId, dto.Cantidad);

        // Cambiar estado a Aprobado (simulando pago en efectivo inmediato para desarrollo)
        compra.Estado = EstadoCompra.Aprobado;
        compra.FechaPago = DateTime.UtcNow;
        await _compraRepository.UpdateEstadoAsync(compra.Id, EstadoCompra.Aprobado);

        return Result.Success(MapToResponseDTO(compra, entradas, sector, evento));
    }

    public async Task<Result<CompraResponseDTO>> ConfirmarPagoAsync(Guid usuarioId, Guid compraId)
    {
        var compra = await _compraRepository.GetByIdAsync(compraId);
        if (compra == null) return Result.Failure<CompraResponseDTO>("Compra no encontrada");
        if (compra.UsuarioId != usuarioId) return Result.Failure<CompraResponseDTO>("No tiene permisos para confirmar esta compra");
        if (compra.Estado != EstadoCompra.PendientePago) return Result.Failure<CompraResponseDTO>("La compra no está pendiente de pago");

        await _compraRepository.UpdateEstadoAsync(compraId, EstadoCompra.Aprobado);
        var updated = await _compraRepository.GetByIdAsync(compraId);
        return Result.Success(MapToResponseDTO(updated!, updated.Entradas, updated.Sector!, updated.Evento!));
    }

    public async Task<Result> CancelarCompraAsync(Guid usuarioId, Guid compraId)
    {
        var compra = await _compraRepository.GetByIdAsync(compraId);
        if (compra == null) return Result.Failure("Compra no encontrada");
        if (compra.UsuarioId != usuarioId) return Result.Failure("No tiene permisos para cancelar esta compra");
        if (compra.Estado != EstadoCompra.PendientePago) return Result.Failure("Solo se pueden cancelar compras pendientes de pago");

        await _reservaTemporalService.LiberarReservaAsync(compraId);
        await _sectorRepository.DecrementarReservadasAsync(compra.SectorId, compra.Cantidad);
        await _compraRepository.UpdateEstadoAsync(compraId, EstadoCompra.Cancelada);

        foreach (var entrada in compra.Entradas)
        {
            await _entradaRepository.UpdateEstadoAsync(entrada.Id, EstadoEntrada.Cancelada);
        }

        return Result.Success();
    }

    public async Task<Result<CompraResponseDTO>> GetByIdAsync(Guid compraId)
    {
        var compra = await _compraRepository.GetByIdAsync(compraId);
        if (compra == null) return Result.Failure<CompraResponseDTO>("Compra no encontrada");
        return Result.Success(MapToResponseDTO(compra, compra.Entradas, compra.Sector!, compra.Evento!));
    }

    public async Task<PagedResult<CompraResponseDTO>> GetByUsuarioAsync(Guid usuarioId, int page, int pageSize)
    {
        var result = await _compraRepository.GetByUsuarioAsync(usuarioId, page, pageSize);
        var items = result.Items.Select(c => MapToResponseDTO(c, c.Entradas, c.Sector!, c.Evento!)).ToList();
        return new PagedResult<CompraResponseDTO>(items, result.PageNumber, result.PageSize, result.TotalCount);
    }

    private static readonly string[] EstadosCompraValidos = ["PendientePago", "Aprobado", "Cancelada"];
    private const int MaxPageSize = 100;

    public async Task<PagedResult<CompraResponseDTO>> GetByEventoAsync(Guid eventoId, DateTime? fechaDesde, DateTime? fechaHasta, string? estado, int page, int pageSize)
    {
        var errorValidacion = ValidarParametrosReporte(page, pageSize, fechaDesde, fechaHasta, estado);
        if (errorValidacion != null) return new PagedResult<CompraResponseDTO>([], 1, pageSize, 0);

        var result = await _compraRepository.GetByEventoAsync(eventoId, fechaDesde, fechaHasta, estado, page, pageSize);
        var items = result.Items.Select(c => MapToResponseDTO(c, c.Entradas, c.Sector!, c.Evento!)).ToList();
        return new PagedResult<CompraResponseDTO>(items, result.PageNumber, result.PageSize, result.TotalCount);
    }

    private static string? ValidarParametrosReporte(
        int page, int pageSize,
        DateTime? fechaDesde, DateTime? fechaHasta,
        string? estado)
    {
        if (page < 1) return "El número de página debe ser mayor a 0";
        if (pageSize < 1) return "El tamaño de página debe ser mayor a 0";
        if (pageSize > MaxPageSize) return $"El tamaño de página no puede exceder {MaxPageSize}";

        if (fechaDesde.HasValue && fechaHasta.HasValue && fechaDesde > fechaHasta)
            return "La fecha 'desde' no puede ser posterior a la fecha 'hasta'";

        if (!string.IsNullOrWhiteSpace(estado) && !EstadosCompraValidos.Contains(estado, StringComparer.OrdinalIgnoreCase))
            return $"El estado debe ser uno de: {string.Join(", ", EstadosCompraValidos)}";

        return null;
    }

    private static CompraResponseDTO MapToResponseDTO(Compra c, IEnumerable<Entrada> entradas, Sector sector, Evento evento) => new()
    {
        Id = c.Id,
        UsuarioId = c.UsuarioId,
        EventoId = c.EventoId,
        EventoNombre = evento.Nombre,
        SectorId = c.SectorId,
        SectorNombre = sector.Nombre,
        Cantidad = c.Cantidad,
        Total = c.Total,
        Estado = c.Estado,
        FechaCreacion = c.FechaCreacion,
        FechaPago = c.FechaPago,
        Entradas = entradas.Select(e => new EntradaEnComprobanteDTO
        {
            Id = e.Id,
            Dni = e.Dni,
            CodigoUnico = e.CodigoUnico,
            Sector = sector.Nombre,
            Precio = sector.PrecioActual
        }).ToList(),
        CodigoQR = $"QR-{c.Id:N}"
    };

    private static string GenerarCodigoUnico() => $"EVT-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
}