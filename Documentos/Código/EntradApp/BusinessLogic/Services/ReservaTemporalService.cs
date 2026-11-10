
using EntradApp.BusinessLogic.Interfaces;
using EntradApp.DataAccess.Entities;
using EntradApp.DataAccess.Repositories.Interfaces;
using EntradApp.Shared.Common;
using EntradApp.Shared.Enums;
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
    private readonly ICompraRepository _compraRepository;
    private readonly IEntradaRepository _entradaRepository;
    private readonly ReservaTemporalOptions _options;

    public ReservaTemporalService(
        IReservaTemporalRepository reservaRepository,
        ISectorRepository sectorRepository,
        ICompraRepository compraRepository,
        IEntradaRepository entradaRepository,
        IOptions<ReservaTemporalOptions> options)
    {
        _reservaRepository = reservaRepository;
        _sectorRepository = sectorRepository;
        _compraRepository = compraRepository;
        _entradaRepository = entradaRepository;
        _options = options.Value;
    }

    public async Task<Result> CrearReservaAsync(
        Guid compraId, Guid sectorId, int cantidad, int duracionMinutos)
    {
        var reservaResult = await _sectorRepository.IncrementarReservadasAsync(
            sectorId, cantidad);

        if (!reservaResult.IsSuccess)
            return Result.Failure(reservaResult.Error!);

        var reserva = new ReservaTemporal
        {
            Id = Guid.NewGuid(),
            CompraId = compraId,
            SectorId = sectorId,
            Cantidad = cantidad,
            FechaCreacion = DateTime.UtcNow,
            ExpiraEn = DateTime.UtcNow.AddMinutes(
                duracionMinutos > 0
                    ? duracionMinutos
                    : _options.DuracionMinutos),
            Expirada = false
        };

        var resultado = await _reservaRepository.CreateAsync(reserva);

        if (!resultado.IsSuccess)
        {
            await _sectorRepository.DecrementarReservadasAsync(
                sectorId, cantidad);

            return Result.Failure(
                resultado.Error ?? "No se pudo crear la reserva temporal");
        }

        return Result.Success();
    }

    public async Task<Result> LiberarReservaAsync(Guid compraId)
    {
        var reserva = await _reservaRepository.GetByCompraIdAsync(compraId);

        if (reserva == null)
            return Result.Success();

        if (!reserva.Expirada)
        {
            var resultado = await _sectorRepository.DecrementarReservadasAsync(
                reserva.SectorId, reserva.Cantidad);

            if (!resultado.IsSuccess)
                return resultado;
        }

        await _reservaRepository.DeleteAsync(reserva.Id);

        return Result.Success();
    }

    public async Task<Result> ConfirmarReservaAsync(Guid compraId)
    {
        var reserva = await _reservaRepository.GetByCompraIdAsync(compraId);

        if (reserva == null || reserva.Expirada ||
            reserva.ExpiraEn <= DateTime.UtcNow)
        {
            return Result.Failure("La reserva no existe o ya expiró");
        }

        var decrementoResult = await _sectorRepository.DecrementarReservadasAsync(
            reserva.SectorId, reserva.Cantidad);

        if (!decrementoResult.IsSuccess)
            return decrementoResult;

        var ventaResult = await _sectorRepository.IncrementarVendidasAsync(
            reserva.SectorId, reserva.Cantidad);

        if (!ventaResult.IsSuccess)
            return ventaResult;

        await _reservaRepository.DeleteAsync(reserva.Id);

        return Result.Success();
    }

    public async Task<Result> LimpiarReservasExpiradasAsync()
    {
        var expiradas = await _reservaRepository.GetExpiradasAsync();

        foreach (var reserva in expiradas)
        {
            var decrementoResult =
                await _sectorRepository.DecrementarReservadasAsync(
                    reserva.SectorId, reserva.Cantidad);

            if (!decrementoResult.IsSuccess)
                continue;

            await _reservaRepository.MarcarExpiradaAsync(reserva.Id);

            var compra = await _compraRepository.GetByIdAsync(reserva.CompraId);

            if (compra != null &&
                compra.Estado == EstadoCompra.PendientePago)
            {
                await _compraRepository.UpdateEstadoAsync(
                    compra.Id, EstadoCompra.Cancelada);

                foreach (var entrada in compra.Entradas)
                {
                    await _entradaRepository.UpdateEstadoAsync(
                        entrada.Id, EstadoEntrada.Cancelada);
                }
            }
        }

        return Result.Success();
    }
}
