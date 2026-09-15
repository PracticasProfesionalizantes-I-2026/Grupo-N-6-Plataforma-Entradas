using EntradApp.BusinessLogic.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EntradApp.API.Middleware;

public class PrecioDinamicoJob : BackgroundService
{
    private readonly ILogger<PrecioDinamicoJob> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _intervalo;

    public PrecioDinamicoJob(
        ILogger<PrecioDinamicoJob> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _intervalo = TimeSpan.FromMinutes(configuration.GetValue<int>("PrecioDinamico:IntervaloMinutos", 5));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var precioService = scope.ServiceProvider.GetRequiredService<IPrecioDinamicoService>();
                await precioService.EvaluarYAplicarIncrementoAsync();
                _logger.LogInformation("Job de precio dinámico ejecutado correctamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en job de precio dinámico");
            }

            await Task.Delay(_intervalo, stoppingToken);
        }
    }
}

public class ReservaTemporalCleanupJob : BackgroundService
{
    private readonly ILogger<ReservaTemporalCleanupJob> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _intervalo;

    public ReservaTemporalCleanupJob(
        ILogger<ReservaTemporalCleanupJob> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _intervalo = TimeSpan.FromMinutes(configuration.GetValue<int>("ReservaTemporal:LimpiezaIntervaloMinutos", 1));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var reservaService = scope.ServiceProvider.GetRequiredService<IReservaTemporalService>();
                await reservaService.LimpiarReservasExpiradasAsync();
                _logger.LogDebug("Limpieza de reservas temporales completada");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en limpieza de reservas temporales");
            }

            await Task.Delay(_intervalo, stoppingToken);
        }
    }
}

public class EventoFinalizadoJob : BackgroundService
{
    private readonly ILogger<EventoFinalizadoJob> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _intervalo = TimeSpan.FromHours(1);

    public EventoFinalizadoJob(ILogger<EventoFinalizadoJob> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<EntradApp.DataAccess.EntradAppDbContext>();
                
                var eventos = await db.Eventos
                    .Where(e => e.Estado == EntradApp.Shared.Enums.EstadoEvento.Aprobado 
                             && e.FechaFin < DateTime.UtcNow)
                    .ToListAsync(stoppingToken);

                foreach (var evento in eventos)
                {
                    evento.Estado = EntradApp.Shared.Enums.EstadoEvento.Finalizado;
                    evento.FechaActualizacion = DateTime.UtcNow;
                }

                if (eventos.Count > 0)
                {
                    await db.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Se finalizaron {Count} eventos", eventos.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en job de finalización de eventos");
            }

            await Task.Delay(_intervalo, stoppingToken);
        }
    }
}