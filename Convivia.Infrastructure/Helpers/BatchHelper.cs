using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Convivia.Infrastructure.Helpers
{
    public static class BatchHelper
    {
        /// <summary>
        /// Procesa una colección en lotes (chunks) y ejecuta la acción por lote con reintentos.
        /// </summary>
        public static async Task ProcessInBatchesAsync<T>(
            IEnumerable<T> items,
            Func<IEnumerable<T>, CancellationToken, Task> processBatchAsync,
            int batchSize = 500,
            int maxRetries = 3,
            TimeSpan? retryDelay = null,
            CancellationToken ct = default)
        {
            if (items == null) return;
            if (processBatchAsync == null) throw new ArgumentNullException(nameof(processBatchAsync));
            if (batchSize <= 0) batchSize = 500;
            retryDelay ??= TimeSpan.FromSeconds(2);

            var list = items as IList<T> ?? items.ToList();
            for (int i = 0; i < list.Count; i += batchSize)
            {
                ct.ThrowIfCancellationRequested();
                var batch = list.Skip(i).Take(batchSize).ToList();
                int attempt = 0;
                while (true)
                {
                    try
                    {
                        await processBatchAsync(batch, ct).ConfigureAwait(false);
                        break;
                    }
                    catch (Exception ex) when (attempt < maxRetries)
                    {
                        attempt++;
                        await Task.Delay(TimeSpan.FromMilliseconds(retryDelay.Value.TotalMilliseconds * attempt), ct).ConfigureAwait(false);
                    }
                }
            }
        }
    }
}