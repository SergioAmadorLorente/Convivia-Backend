using Google.Cloud.Firestore;

namespace Convivia.Infrastructure.Models
{
    /// <summary>
    /// Modelo de persistencia para Firestore
    /// Coincide exactamente con la estructura en Firebase
    /// </summary>
    [FirestoreData]
    public class FireStoreInvitacion
    {
        [FirestoreProperty("Id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [FirestoreProperty("UsuarioSolicitanteId")]
        public string UsuarioSolicitanteId { get; set; } = string.Empty;

        [FirestoreProperty("UsuarioInvitadoId")]
        public string UsuarioInvitadoId { get; set; } = string.Empty;

        [FirestoreProperty("EspacioId")]
        public string EspacioId { get; set; } = string.Empty;

        [FirestoreProperty("Mensaje")]
        public string? Mensaje { get; set; }

        [FirestoreProperty("Fecha")]
        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        [FirestoreProperty("Estado")]
        public string Estado { get; set; } = "pendiente";
    }
}
