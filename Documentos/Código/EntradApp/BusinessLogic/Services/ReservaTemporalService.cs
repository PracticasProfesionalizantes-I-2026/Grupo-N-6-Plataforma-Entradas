using EntradApp.BusinessLogic.Interfaces;
using EntradApp.DataAccess.Entities;
using EntradApp.DataAccess.Repositories.Interfaces;
using EntradApp.Shared.Common;
using EntradApp.Shared.Exceptions;
using Microsoft.Extensions.Options;

namespace EntradApp.BusinessLogic.Services;

public class ReservaTemporalOptions
{
    public int DuracionMinutos { get; set; } = 10;
    public int LimpiezaIntervaloMinutos { get; set; } = 1;
}

public class ReservaTemporalService : IReservaTemporalService
{
    private readonly IReservaTemporalRepository _reservaRepository;
    private readonly ISectorRepository _sectorRepository;
    private readonly ReservaTemporalOptions _options;

    public ReservaTemporalService(
        IReservaTemporalRepository reservaRepository,
        ISectorRepository sectorRepository,
        IOptions<ReservaTemporalOptions> options)
    {
        _reservaRepository = reservaRepository;
        _sectorRepository = sectorRepository;
        _options = options.Value;
    }

    public async Task<Result> CrearReservaAsync(Guid compraId, Guid sectorId, int cantidad, int duracionMinutos)
    {
        var sector = await _sectorRepository.GetByIdAsync(sectorId);
        if (sector == null) return Result.Failure("Sector no encontrado");
        
        if (sector.StockDisponible < cantidad)
            return Result.Failure("Stock insuficiente para la reserva");

        await _sectorRepository.IncrementarReservadasAsync(sectorId, cantidad);

        var reserva = new ReservaTemporal
        {
            Id = Guid.NewGuid(),
            CompraId = compraId,
            SectorId = sectorId,
            Cantidad = cantidad,
            FechaCreacion = DateTime.UtcNow,
            ExpiraEn = DateTime.UtcNow.AddMinutes(duracionMinutos > 0 ? duracionMinutos : _options.DuracionMinutos),
            Expirada = false
        };

        await _reservaRepository.CreateAsync(reserva);
        return Result.Success();
    }

    public async Task<Result> LiberarReservaAsync(Guid compraId)
    {
        var reserva = await _reservaRepository.GetByCompraIdAsync(compraId);
        if (reserva == null) return Result.Success();

        await _sectorRepository.DecrementarReservadasAsync(reserva.SectorId, reserva.Cantidad);
        await _reservaRepository.DeleteAsync(reserva.Id);
        return Result.Success();
    }

    public async Task<Result> LimpiarReservasExpiradasAsync()
    {
        var expiradas = await _reservaRepository.GetExpiradasAsync();
        
        foreach (var reserva in expiradas)
        {
            await _sectorRepository.DecrementarReservadasAsync(reserva.SectorId, reserva.Cantidad);
            await _reservaRepository.MarcarExpiradaAsync(reserva.Id);
        }
        
        return Result.Success();
    }
}