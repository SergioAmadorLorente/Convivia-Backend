using Google.Cloud.Firestore;

namespace Convivia.Infrastructure.Models
{
    /// <summary>
    /// Modelo de persistencia para Firestore
    /// Coincide exactamente con la estructura en Firebase
    /// </summary>
    [FirestoreData]
    public class FireStoreSala
    {
        [FirestoreProperty("Id")]
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        [FirestoreProperty("Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [FirestoreProperty("Descripcion")]
        public string? Descripcion { get; set; }

        [FirestoreProperty("IdEspacio")]
        public string IdEspacio { get; set; } = string.Empty;
    }
}