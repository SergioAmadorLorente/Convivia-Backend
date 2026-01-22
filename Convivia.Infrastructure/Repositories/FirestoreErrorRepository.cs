using Google.Cloud.Firestore;
using Convivia.Shared.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Convivia.Infrastructure.Repositories
{
    public class FirestoreErrorRepository : IErrorRepository
    {
        private readonly FirestoreDb _db;
        private readonly string _collectionName = "api-errors";

        public FirestoreErrorRepository(FirestoreDb db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task SaveAsync(ErrorRecord record, CancellationToken cancellationToken = default)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));

            // Construir documento con campos básicos
            var docRef = _db.Collection(_collectionName).Document();

            var data = new Dictionary<string, object?>
            {
                ["correlationId"] = record.CorrelationId,
                ["traceId"] = record.TraceId,
                ["status"] = record.Status,
                ["message"] = record.Message,
                ["route"] = record.Route,
                ["timestampUtc"] = record.TimestampUtc,
                ["stack"] = TruncateString(record.Stack, 10000)
            };

            // Añadir entity/key si existen
            if (!string.IsNullOrEmpty(record.Entity))
                data["entity"] = record.Entity;

            if (!string.IsNullOrEmpty(record.Key))
                data["key"] = record.Key;

            // Convertir ValidationErrors (IReadOnlyDictionary<string,string[]>) a un Dictionary<string, object?>
            if (record.ValidationErrors != null && record.ValidationErrors.Any())
            {
                // Limitar número de entradas para evitar documentos enormes
                const int maxEntries = 200;
                var entries = record.ValidationErrors.Take(maxEntries);

                var validationDict = new Dictionary<string, object?>();
                foreach (var kv in entries)
                {
                    // Sanear cada valor (array de strings) y truncar cada string
                    var sanitized = kv.Value?.Select(v => TruncateString(v, 1000)).ToArray() ?? Array.Empty<string>();
                    validationDict[kv.Key] = sanitized;
                }

                data["validationErrors"] = validationDict;
            }

            // Guardar en Firestore
            await docRef.SetAsync(data, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        private static string? TruncateString(string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}
