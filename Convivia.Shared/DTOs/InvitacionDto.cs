using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Convivia.Shared.DTOs
{
    [FirestoreData]
    public class InvitacionDto
    {
        public InvitacionDto() { }

        [FirestoreProperty("Id")]
        public string Id { get; set; }

        [FirestoreProperty("UsuarioSolicitanteId")]
        public string UsuarioSolicitanteId { get; set; }

        [FirestoreProperty("UsuarioInvitadoId")]
        public string UsuarioInvitadoId { get; set; }

        [FirestoreProperty("EspacioId")]
        public string EspacioId { get; set; }

        [FirestoreProperty("Mensaje")]
        public string Mensaje { get; set; }

        [FirestoreProperty("Fecha")]
        public DateTime Fecha { get; set; }

        [FirestoreProperty("Estado")]
        public string Estado { get; set; }
    }
}