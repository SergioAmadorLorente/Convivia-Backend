using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Convivia.Infrastructure.HostedServices
{
    /// <summary>
    /// Servicio en segundo plano que elimina automáticamente las facturas completamente pagadas.
    /// MODO PRUEBAS: 30 segundos (luego se cambiará a 15 días).
    /// </summary>
    public class FacturaCleanupBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<FacturaCleanupBackgroundService> _logger;
        
        // MODO PRUEBAS: 30 segundos
        private static readonly TimeSpan UmbralBorrado = TimeSpan.FromSeconds(30);
        // Intervalo de comprobación: cada 15 segundos
        private static readonly TimeSpan Intervalo = TimeSpan.FromSeconds(15);

        public FacturaCleanupBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<FacturaCleanupBackgroundService> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Console.WriteLine($"[FacturaCleanup] MODO PRUEBA ACTIVO. Umbral: {UmbralBorrado.TotalSeconds}s, Intervalo: {Intervalo.TotalSeconds}s");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("[FacturaCleanup] BackgroundService INICIADO.");

            // Esperar 5 segundos al arranque
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
                    await Task.Delay(Intervalo, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            Console.WriteLine("[FacturaCleanup] BackgroundService DETENIDO.");
        }

        private async Task EjecutarLimpiezaAsync(CancellationToken ct)
        {
            var umbral = DateTime.UtcNow.Subtract(UmbralBorrado);

            Console.WriteLine($"[FacturaCleanup] Iniciando ciclo. Umbral: facturas pagadas <= {umbral:yyyy-MM-dd HH:mm:ss} UTC");

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<FirestoreDb>();

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
