using Google.Cloud.Firestore;

namespace Convivia.Infrastructure.Models
{
    [FirestoreData]
    public class FireStoreReserva
    {
        [FirestoreProperty("idReserva")]
        public string idReserva { get; set; } = string.Empty;

        [FirestoreProperty("description")]
        public string? description { get; set; } = string.Empty;

        [FirestoreProperty("startTime")]
        public DateTime startTime { get; } = DateTime.UtcNow;

        [FirestoreProperty("endTime")]
        public DateTime? endTime { get; set; }

        [FirestoreProperty("idSala")]
        public string idSala { get; set; } = string.Empty;

        [FirestoreProperty("idUser")]
        public string idUser { get; set; } = string.Empty;
    }
}
