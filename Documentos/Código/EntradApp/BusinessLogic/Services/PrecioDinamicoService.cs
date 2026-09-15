using EntradApp.BusinessLogic.Interfaces;
using EntradApp.DataAccess.Entities;
using EntradApp.DataAccess.Repositories.Interfaces;
using EntradApp.Shared.Common;
using EntradApp.Shared.Enums;
using Microsoft.Extensions.Options;

namespace EntradApp.BusinessLogic.Services;

public class PrecioDinamicoOptions
{
    public int UmbralOcupacion { get; set; } = 80;
    public int PorcentajeIncremento { get; set; } = 20;
}

public class PrecioDinamicoService : IPrecioDinamicoService
{
    private readonly ISectorRepository _sectorRepository;
    private readonly IEventoRepository _eventoRepository;
    private readonly PrecioDinamicoOptions _options;

    public PrecioDinamicoService(
        ISectorRepository sectorRepository,
        IEventoRepository eventoRepository,
        IOptions<PrecioDinamicoOptions> options)
    {
        _sectorRepository = sectorRepository;
        _eventoRepository = eventoRepository;
        _options = options.Value;
    }

    public async Task<Result> EvaluarYAplicarIncrementoAsync()
    {
        var eventosAprobados = await _eventoRepository.GetAprobadosAsync(1, int.MaxValue);
        
        foreach (var evento in eventosAprobados.Items)
        {
            var sectores = await _sectorRepository.GetByEventoAsync(evento.Id);
            
            foreach (var sector in sectores.Where(s => !s.EsBorrado))
            {
                var ocupacion = sector.PorcentajeOcupacion;
                
                if (ocupacion > _options.UmbralOcupacion && sector.PrecioActual == sector.PrecioBase)
                {
                    var nuevoPrecio = sector.PrecioBase * (1 + _options.PorcentajeIncremento / 100m);
                    sector.PrecioActual = Math.Round(nuevoPrecio, 2);
                    await _sectorRepository.UpdateAsync(sector);
                    // Log auditoría aquí
                }
                // Si ya incrementado, no hacer nada (idempotencia)
            }
        }
        
        return Result.Success();
    }
}