using Convivia.Domain.Entities;
using Google.Cloud.Firestore;
using System.Text.Json.Serialization;
namespace Convivia.Domain.Entities
{


    [FirestoreData]
    public class FireStoreUsuarioEspacio
    {
        [FirestoreDocumentId]
        public string Id_UsuarioEspacio { get; set; } = Guid.NewGuid().ToString();

        [FirestoreProperty("Ausente")]
        public bool Ausente { get; set; }

        [FirestoreProperty("Karma")]
        public int Karma { get; set; }

        [FirestoreProperty("Rol")]
        public string Rol { get; set; }

        [FirestoreProperty("EspacioId")]
        public string EspacioId { get; set; }

        [FirestoreProperty("UsuarioId")]
        public string Usuarioíd { get; set; }

        [FirestoreProperty("tareasId")]
        public List<string> tareasId { get; set; } = new();
   
        [FirestoreProperty("PermisoId")]
        public string PermisoId { get; set; }

        [FirestoreProperty("FacturasId")]
        public List<string> FacturasId { get; set; } = new();

    }
}