using Google.Cloud.Firestore;

namespace Convivia.Infrastructure.Models
{
    [FirestoreData]
    public class FireStoreErrorRecord
    {
        [FirestoreProperty("correlationId")]
        public string CorrelationId { get; set; } = string.Empty;

        [FirestoreProperty("traceId")]
        public string? TraceId { get; set; }

        [FirestoreProperty("status")]
        public int Status { get; set; }

        [FirestoreProperty("message")]
        public string? Message { get; set; }

        [FirestoreProperty("route")]
        public string? Route { get; set; }

        [FirestoreProperty("timestampUtc")]
        public DateTime TimestampUtc { get; set; }

        [FirestoreProperty("stack")]
        public string? Stack { get; set; }

        [FirestoreProperty("userId")]
        public string? UserId { get; set; }
    }
}
