using Google.Cloud.Firestore;
using System.Collections.Generic;

namespace Convivia.Domain.Models
{
    [FirestoreData]
    public class EspacioPersist
    {
        [FirestoreProperty]
        public string Id_Espacio { get; set; }
        [FirestoreProperty]
        public string Nombre { get; set; }
        [FirestoreProperty]
        public string? Direccion { get; set; }
        [FirestoreProperty]
        public List<string>? SalaIds { get; set; }
        [FirestoreProperty]
        public List<string>? UsuarioEspacioIds { get; set; }
        [FirestoreProperty]
        public List<string>? PeticionIds { get; set; }
        [FirestoreProperty]
        public List<string>? InvitacionIds { get; set; }
    }
}