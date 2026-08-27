using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Convivia.Infrastructure.HostedServices
{
    /// <summary>
    /// Servicio en segundo plano que elimina automáticamente las facturas completamente pagadas
    /// una vez transcurridos los días configurados desde su fecha de pago (FechaPago).
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
            
            // Si el intervalo es muy pequeño (ej. 0.01 h = 36 seg), aseguramos mínimo 10 segundos
            var totalSeconds = intervaloHoras * 3600;
            if (totalSeconds < 10) totalSeconds = 10;
            _intervalo = TimeSpan.FromSeconds(totalSeconds);

            Console.WriteLine($"[FacturaCleanup] Servicio registrado. DiasHastaBorrado: {_diasHastaBorrado}, Intervalo: {_intervalo.TotalSeconds}s");
            _logger.LogInformation(
                "[FacturaCleanup] Configurado: borrar facturas pagadas hace mas de {Dias} dias. Intervalo: {Intervalo}.",
                _diasHastaBorrado, _intervalo);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("[FacturaCleanup] BackgroundService INICIADO y corriendo.");
            _logger.LogInformation("[FacturaCleanup] BackgroundService INICIADO y corriendo.");

            // Esperar 5 segundos al arranque antes del primer ciclo para que el resto de la app termine de inicializarse
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }

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
                    Console.WriteLine($"[FacturaCleanup] ERROR en ciclo: {ex.Message}");
                    _logger.LogError(ex, "[FacturaCleanup] Error durante la limpieza.");
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

            Console.WriteLine("[FacturaCleanup] BackgroundService DETENIDO.");
            _logger.LogInformation("[FacturaCleanup] BackgroundService DETENIDO.");
        }

        private async Task EjecutarLimpiezaAsync(CancellationToken ct)
        {
            var umbral = DateTime.UtcNow.AddDays(-_diasHastaBorrado);

            Console.WriteLine($"[FacturaCleanup] Iniciando ciclo. Umbral: facturas pagadas <= {umbral:yyyy-MM-dd HH:mm:ss} UTC");
            _logger.LogInformation("[FacturaCleanup] Iniciando ciclo. Umbral: facturas pagadas <= {Umbral:yyyy-MM-dd HH:mm:ss} UTC", umbral);

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<FirestoreDb>();

            // Obtener todas las facturas de todas las subcolecciones directamente
            var snapshot = await db.CollectionGroup("facturas").GetSnapshotAsync(ct);
            Console.WriteLine($"[FacturaCleanup] Total facturas encontradas en Firestore: {snapshot.Count}");

            int totalEliminadas = 0;

            foreach (var doc in snapshot.Documents)
            {
                try
                {
                    var data = doc.ToDictionary();
                    if (data == null) continue;

                    // Leer Pagado (tolerante a mayúsculas/minúsculas)
                    bool pagado = false;
                    if (data.TryGetValue("Pagado", out var pVal) || data.TryGetValue("pagado", out pVal))
                    {
                        if (pVal is bool b) pagado = b;
                    }

                    if (!pagado) continue;

                    // Leer FechaPago
                    DateTime? fechaPago = null;
                    if (data.TryGetValue("FechaPago", out var fpVal) || data.TryGetValue("fechaPago", out fpVal))
                    {
                        fechaPago = ConvertToUtcDateTime(fpVal);
                    }

                    // Leer FechaCreacion
                    DateTime? fechaCreacion = null;
                    if (data.TryGetValue("FechaCreacion", out var fcVal) || data.TryGetValue("fechaCreacion", out fcVal))
                    {
                        fechaCreacion = ConvertToUtcDateTime(fcVal);
                    }
                    else if (doc.CreateTime.HasValue)
                    {
                        fechaCreacion = doc.CreateTime.Value.ToDateTime();
                    }

                    var fechaReferencia = fechaPago ?? fechaCreacion ?? DateTime.UtcNow;
                    var nombre = data.TryGetValue("Nombre", out var nVal) || data.TryGetValue("nombre", out nVal) 
                        ? nVal?.ToString() ?? "Sin nombre" 
                        : "Sin nombre";

                    if (fechaReferencia <= umbral)
                    {
                        var espacioId = doc.Reference.Parent?.Parent?.Id ?? "desconocido";
                        await doc.Reference.DeleteAsync(cancellationToken: ct);
                        
                        var msg = $"[FacturaCleanup] ELIMINADA factura '{doc.Id}' ('{nombre}') del espacio '{espacioId}'. Pagado: true, FechaRef: {fechaReferencia:yyyy-MM-dd HH:mm:ss} UTC, Umbral: {umbral:yyyy-MM-dd HH:mm:ss} UTC";
                        Console.WriteLine(msg);
                        _logger.LogInformation(msg);
                        
                        totalEliminadas++;
                    }
                    else
                    {
                        Console.WriteLine($"[FacturaCleanup] Factura '{doc.Id}' ('{nombre}') está pagada pero fechaRef ({fechaReferencia:yyyy-MM-dd HH:mm:ss}) > umbral ({umbral:yyyy-MM-dd HH:mm:ss}). Aún no toca borrar.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[FacturaCleanup] Error procesando factura '{doc.Id}': {ex.Message}");
                    _logger.LogError(ex, "[FacturaCleanup] Error procesando factura '{DocId}'.", doc.Id);
                }
            }

            Console.WriteLine($"[FacturaCleanup] Ciclo completado. Facturas revisadas: {snapshot.Count}, Eliminadas: {totalEliminadas}");
        }

        private static DateTime? ConvertToUtcDateTime(object? value)
        {
            if (value == null) return null;
            if (value is Timestamp ts) return ts.ToDateTime();
            if (value is DateTime dt) return dt.Kind == DateTimeKind.Utc ? dt : dt.ToUniversalTime();
            if (value is string s && DateTime.TryParse(s, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out var parsed))
                return parsed;
            return null;
        }
    }
}
