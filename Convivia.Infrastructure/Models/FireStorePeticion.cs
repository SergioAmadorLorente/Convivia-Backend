using Google.Cloud.Firestore;

namespace Convivia.Infrastructure.Models
{
    /// <summary>
    /// Modelo de persistencia para Firestore
    /// Coincide exactamente con la estructura en Firebase
    /// </summary>
    [FirestoreData]
    public class FireStorePeticion
    {
        [FirestoreProperty("Id")]
        public string Id { get; set; } = string.Empty;

        [FirestoreProperty("Mensaje")]
        public string Mensaje { get; set; } = string.Empty;

        [FirestoreProperty("Fecha")]
        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        [FirestoreProperty("Estado")]
        public string Estado { get; set; } = "pendiente";

        [FirestoreProperty("SolicitanteId")]
        public string IdSolicitante { get; set; } = string.Empty;

        [FirestoreProperty("EspacioId")]
        public string IdEspacio { get; set; } = string.Empty;
    }
}
