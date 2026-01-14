// Convivia.Shared.Helpers/BatchHelper.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Convivia.Shared.Helpers
{
    public static class BatchHelper
    {
        /// <summary>
        /// Procesa una colección en lotes (chunks) y ejecuta la acción por lote con reintentos y backoff exponencial.
        /// </summary>
        /// <typeparam name="T">Tipo de elemento</typeparam>
        /// <param name="items">Colección de entrada (puede ser null)</param>
        /// <param name="processBatchAsync">Función que procesa un lote; recibe IReadOnlyList para evitar modificaciones</param>
        /// <param name="batchSize">Tamaño máximo por lote (por defecto 500)</param>
        /// <param name="maxRetries">Número máximo de reintentos por lote (por defecto 3)</param>
        /// <param name="initialRetryDelay">Delay inicial entre reintentos (por defecto 2s). Se aplica backoff exponencial.</param>
        /// <param name="logger">Logger opcional para trazas</param>
        /// <param name="ct">CancellationToken</param>
        public static async Task ProcessInBatchesAsync<T>(
            IEnumerable<T>? items,
            Func<IReadOnlyList<T>, CancellationToken, Task> processBatchAsync,
            int batchSize = 500,
            int maxRetries = 3,
            TimeSpan? initialRetryDelay = null,
            ILogger? logger = null,
            CancellationToken ct = default)
        {
            if (processBatchAsync == null) throw new ArgumentNullException(nameof(processBatchAsync));
            if (batchSize <= 0) throw new ArgumentOutOfRangeException(nameof(batchSize));
            if (maxRetries < 0) throw new ArgumentOutOfRangeException(nameof(maxRetries));

            initialRetryDelay ??= TimeSpan.FromSeconds(2);
            var list = items as IList<T> ?? (items?.ToList() ?? new List<T>());

            if (!list.Any())
            {
                logger?.LogDebug("BatchHelper: no items to process.");
                return;
            }

            for (int i = 0; i < list.Count; i += batchSize)
            {
                ct.ThrowIfCancellationRequested();

                var batch = list.Skip(i).Take(batchSize).ToList().AsReadOnly();
                int attempt = 0;
                TimeSpan delay = initialRetryDelay.Value;

                while (true)
                {
                    ct.ThrowIfCancellationRequested();
                    try
                    {
                        logger?.LogDebug("BatchHelper: processing batch {BatchIndex} (size {BatchSize}), attempt {Attempt}",
                            i / batchSize, batch.Count, attempt + 1);

                        await processBatchAsync(batch, ct).ConfigureAwait(false);
                        logger?.LogDebug("BatchHelper: batch {BatchIndex} processed successfully", i / batchSize);
                        break;
                    }
                    catch (OperationCanceledException)
                    {
                        // Propagar cancelación inmediatamente
                        logger?.LogInformation("BatchHelper: cancellation requested during batch {BatchIndex}", i / batchSize);
                        throw;
                    }
                    catch (Exception ex)
                    {
                        attempt++;
                        logger?.LogWarning(ex, "BatchHelper: error processing batch {BatchIndex} attempt {Attempt}", i / batchSize, attempt);

                        if (attempt > maxRetries)
                        {
                            logger?.LogError(ex, "BatchHelper: exhausted retries for batch {BatchIndex}", i / batchSize);
                            throw; // re-lanzar la excepción final
                        }

                        // Backoff exponencial con jitter pequeño
                        var jitter = TimeSpan.FromMilliseconds(new Random().Next(0, 200));
                        var wait = delay + jitter;
                        logger?.LogInformation("BatchHelper: retrying batch {BatchIndex} after {Delay} (attempt {Attempt})", i / batchSize, wait, attempt);
                        await Task.Delay(wait, ct).ConfigureAwait(false);

                        // aumentar delay exponencialmente
                        delay = TimeSpan.FromMilliseconds(Math.Min(delay.TotalMilliseconds * 2, TimeSpan.FromMinutes(1).TotalMilliseconds));
                    }
                }
            }
        }
    }
}