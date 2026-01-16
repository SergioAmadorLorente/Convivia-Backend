using Google.Cloud.Firestore;
using Convivia.Shared.Contracts;

namespace Convivia.Infrastructure.Repositories
{
    public class FirestoreErrorRepository
    {
        private readonly FirestoreDb _db;
        private readonly string _collectionName = "api-errors";

        public FirestoreErrorRepository(FirestoreDb db)
        {
            _db = db;
        }

        public async Task SaveAsync(ErrorRecord record, CancellationToken cancellationToken = default)
        {
            var doc = _db.Collection(_collectionName).Document();
            var data = new Dictionary<string, object?>
            {
                ["correlationId"] = record.CorrelationId,
                ["traceId"] = record.TraceId,
                ["status"] = record.Status,
                ["message"] = record.Message,
                ["route"] = record.Route,
                ["timestampUtc"] = record.TimestampUtc,
                ["stack"] = record.Stack
            };

            await doc.SetAsync(data, cancellationToken: cancellationToken);
        }
    }
}
