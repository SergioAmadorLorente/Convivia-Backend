using Google.Cloud.Firestore;

namespace Convivia.Infrastructure.Models
{
    /// <summary>
    /// Modelo de persistencia para Firestore
    /// Coincide exactamente con la estructura en Firebase
    /// </summary>
    [FirestoreData]
    public class FireStorePermiso
    {
        [FirestoreProperty("Id")]
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        [FirestoreProperty("Rol")]
        public string Rol { get; set; } = string.Empty;

        [FirestoreProperty("CrearTarea")]
        public bool CrearTarea { get; set; }

        [FirestoreProperty("EliminarTarea")]
        public bool EliminarTarea { get; set; }

        [FirestoreProperty("EditarTarea")]
        public bool EditarTarea { get; set; }

        [FirestoreProperty("AñadirUsuario")]
        public bool AñadirUsuario { get; set; }

        [FirestoreProperty("EliminarUsuario")]
        public bool EliminarUsuario { get; set; }

        [FirestoreProperty("AsignarTarea")]
        public bool AsignarTarea { get; set; }

        [FirestoreProperty("AsignarseTarea")]
        public bool AsignarseTarea { get; set; }
    }
}
