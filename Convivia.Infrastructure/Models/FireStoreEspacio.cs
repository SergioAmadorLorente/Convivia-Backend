using Google.Cloud.Firestore;

namespace Convivia.Infrastructure.Models
{
    /// <summary>
    /// Modelo de persistencia para Firestore
    /// Coincide exactamente con la estructura en Firebase
    /// </summary>
    [FirestoreData]
    public class FireStoreEspacio
    {
        [FirestoreProperty("Id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [FirestoreProperty("Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [FirestoreProperty("Direccion")]
        public string? Direccion { get; set; }

        [FirestoreProperty("UsuarioEspacioIds")]
        public List<string> UsuarioEspaciosIds { get; set; } = new List<string>();
    }
}
