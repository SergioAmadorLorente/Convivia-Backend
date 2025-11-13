using Google.Cloud.Firestore;
using System;

namespace AuthApiDemo.Models
{
    [FirestoreData]
    public class InvitacionPersist
    {
        [FirestoreProperty("Id")]
        public string Id { get; set; } = string.Empty;

        [FirestoreProperty("UsuarioSolicitante")]
        public string UsuarioSolicitante { get; set; } = string.Empty;

        [FirestoreProperty("UsuarioInvitado")]
        public string UsuarioInvitado { get; set; } = string.Empty;

        [FirestoreProperty("Espacio")]
        public string Espacio { get; set; } = string.Empty;

        [FirestoreProperty("Mensaje")]
        public string Mensaje { get; set; } = string.Empty;

        [FirestoreProperty("Fecha")]
        public DateTime Fecha { get; set; }

        [FirestoreProperty("Estado")]
        public string Estado { get; set; } = "pendiente";
    }
}