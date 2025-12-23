using Convivia.Domain.Entities;
using Google.Cloud.Firestore;
using System.Text.Json.Serialization;

namespace Convivia.Infrastructure.Models
{


    [FirestoreData]
    public class FireStoreUsuarioEspacio
    {
        [FirestoreProperty("Id")]
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        [FirestoreProperty("Ausente")]
        public bool Ausente { get; set; }

        [FirestoreProperty("Karma")]
        public int Karma { get; set; }

        [FirestoreProperty("Rol")]
        public string Rol { get; set; }

        [FirestoreProperty("EspacioRef")]
        public string EspacioId { get; set; }

        [FirestoreProperty("UsuarioRef")]
        public string UsuarioId { get; set; }

        [FirestoreProperty("tareas")]
        public List<string> TareasId { get; set; } = new();

        [FirestoreProperty("PermisoRef")]
        public string PermisoId { get; set; }

        [FirestoreProperty("FacturasRefs")]
        public List<string> FacturasId { get; set; } = new();

    }
}