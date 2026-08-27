using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Convivia.Infrastructure.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Convivia.Infrastructure.HostedServices
{
    /// <summary>
    /// Servicio en segundo plano que elimina automaticamente las facturas completamente pagadas
    /// una vez transcurridos los dias configurados desde su fecha de pago (FechaPago).
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
                "[FacturaCleanup] Configurado: borrar facturas pagadas hace mas de {Dias} dias. Intervalo de comprobacion: {Intervalo}.",
                _diasHastaBorrado, _intervalo);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[FacturaCleanup] Servicio de limpieza iniciado.");

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
                "[FacturaCleanup] Iniciando ciclo de limpieza. Umbral: facturas pagadas antes de {Umbral:yyyy-MM-dd HH:mm:ss} UTC.",
                umbral);

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<FirestoreDb>();

            // Query CollectionGroup("facturas") busca todas las facturas en todas las subcolecciones directamente
            var snapshot = await db.CollectionGroup("facturas").GetSnapshotAsync(ct);
            _logger.LogInformation("[FacturaCleanup] Total facturas encontradas en BD: {Count}", snapshot.Count);

            int totalEliminadas = 0;

            foreach (var doc in snapshot.Documents)
            {
                try
                {
                    var factura = doc.ConvertTo<FireStoreFactura>();
                    if (factura == null) continue;

                    var fechaReferencia = factura.FechaPago ?? factura.FechaCreacion;

                    if (factura.Pagado && fechaReferencia <= umbral)
                    {
                        var espacioId = doc.Reference.Parent?.Parent?.Id ?? "desconocido";
                        await doc.Reference.DeleteAsync(cancellationToken: ct);
                        _logger.LogInformation(
                            "[FacturaCleanup] Eliminada factura '{FacturaId}' ('{Nombre}') del espacio '{EspacioId}'. FechaPago: {FechaPago}, FechaCreacion: {FechaCreacion}.",
                            doc.Id, factura.Nombre, espacioId, factura.FechaPago, factura.FechaCreacion);
                        totalEliminadas++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[FacturaCleanup] Error procesando factura doc '{DocId}'.", doc.Id);
                }
            }

            _logger.LogInformation(
                "[FacturaCleanup] Ciclo completado. Total revisadas: {Revisadas}. Total eliminadas: {Eliminadas}.",
                snapshot.Count, totalEliminadas);
        }
    }
}
