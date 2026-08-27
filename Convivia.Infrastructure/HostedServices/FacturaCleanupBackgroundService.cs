using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Convivia.Application.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Convivia.Infrastructure.HostedServices
{
    /// <summary>
    /// Servicio en segundo plano que elimina automaticamente las facturas completamente pagadas
    /// una vez transcurridos los dias configurados desde su fecha de pago (FechaPago).
    ///
    /// Configuracion en appsettings.json:
    ///   "FacturaCleanup": {
    ///     "DiasHastaBorrado": 15,    // dias desde FechaPago hasta borrar. Default: 15
    ///     "IntervaloHoras": 24       // cada cuantas horas se ejecuta el job. Default: 24
    ///   }
    ///
    /// Para testear manualmente sin esperar 15 dias reales, usa en appsettings.Development.json:
    ///   "FacturaCleanup": { "DiasHastaBorrado": 0, "IntervaloHoras": 0.01 }
    /// Con IntervaloHoras=0.01 (~36 segundos) el job corre casi al arrancar.
    /// Con DiasHastaBorrado=0 borrara todas las facturas pagadas inmediatamente.
    /// </summary>
    public class FacturaCleanupBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<FacturaCleanupBackgroundService> _logger;
        private readonly int _diasHastaBorrado;
        private readonly TimeSpan _intervalo;

        public FacturaCleanupBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<FacturaCleanupBackgroundService> logger,
            IConfiguration configuration)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _diasHastaBorrado = int.TryParse(configuration["FacturaCleanup:DiasHastaBorrado"], out var dias) ? dias : 15;
            var intervaloHoras = double.TryParse(configuration["FacturaCleanup:IntervaloHoras"], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var horas) ? horas : 24.0;
            _intervalo = TimeSpan.FromHours(intervaloHoras);

            _logger.LogInformation(
                "[FacturaCleanup] Configurado: borrar facturas pagadas hace mas de {Dias} dias. Intervalo: {Intervalo}.",
                _diasHastaBorrado, _intervalo);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[FacturaCleanup] Servicio iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await EjecutarLimpiezaAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[FacturaCleanup] Error durante la limpieza. Se reintentara en el proximo ciclo.");
                }

                try
                {
                    await Task.Delay(_intervalo, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            _logger.LogInformation("[FacturaCleanup] Servicio detenido.");
        }

        private async Task EjecutarLimpiezaAsync(CancellationToken ct)
        {
            var umbral = DateTime.UtcNow.AddDays(-_diasHastaBorrado);

            _logger.LogInformation(
                "[FacturaCleanup] Iniciando ciclo de limpieza. Umbral: facturas pagadas antes de {Umbral} UTC.",
                umbral);

            // Usamos un scope porque IFacturaRepository e IEspacioRepository son Scoped
            using var scope = _scopeFactory.CreateScope();
            var facturaRepo = scope.ServiceProvider.GetRequiredService<IFacturaRepository>();
            var espacioRepo = scope.ServiceProvider.GetRequiredService<IEspacioRepository>();

            // Obtenemos todos los espacios para iterar sus subcolecciones de facturas
            var espacios = await espacioRepo.GetAllAsync(ct);
            if (espacios == null)
            {
                _logger.LogInformation("[FacturaCleanup] No se encontraron espacios. Nada que limpiar.");
                return;
            }

            var espaciosList = espacios.ToList();
            int totalEliminadas = 0;

            foreach (var espacio in espaciosList)
            {
                if (string.IsNullOrWhiteSpace(espacio.Id)) continue;

                try
                {
                    var facturasAntiguas = await facturaRepo.GetPagadasAntiguas(espacio.Id, umbral, ct);
                    var facturasLista = facturasAntiguas.ToList();

                    foreach (var factura in facturasLista)
                    {
                        try
                        {
                            await facturaRepo.DeleteAsync(espacio.Id, factura.Id, ct);
                            _logger.LogInformation(
                                "[FacturaCleanup] Eliminada factura '{FacturaId}' ('{Nombre}') del espacio '{EspacioId}'. FechaPago: {FechaPago} UTC.",
                                factura.Id, factura.Nombre, espacio.Id, factura.FechaPago);
                            totalEliminadas++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex,
                                "[FacturaCleanup] Error al eliminar factura '{FacturaId}' del espacio '{EspacioId}'.",
                                factura.Id, espacio.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "[FacturaCleanup] Error al procesar espacio '{EspacioId}'. Se continua con el siguiente.",
                        espacio.Id);
                }
            }

            _logger.LogInformation(
                "[FacturaCleanup] Ciclo completado. Espacios procesados: {Espacios}. Facturas eliminadas: {Eliminadas}.",
                espaciosList.Count, totalEliminadas);
        }
    }
}

