using Google.Cloud.Firestore;

namespace Convivia.Infrastructure.Models
{
    /// <summary>
    /// Modelo de persistencia para Firestore
    /// Coincide exactamente con la estructura en Firebase
    /// </summary>
    [FirestoreData]
    public class FireStoreUsuario
    {
        [FirestoreProperty("Id")]
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        [FirestoreProperty("Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [FirestoreProperty("Email")]
        public string Email { get; set; } = string.Empty;

        [FirestoreProperty("Password")]
        public string Password { get; set; } = string.Empty;

        [FirestoreProperty("Telefono")]
        public string? Telefono { get; set; }

        [FirestoreProperty("Premium")]
        public bool Premium { get; set; }

        [FirestoreProperty("FechaRegistro")]
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    }
}
