using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;

namespace Convivia.Domain.Models
{
    [FirestoreData]
    public class UsuarioPersist
    {
        [FirestoreProperty]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [FirestoreProperty]
        public string Nombre { get; set; } = default!;

        [FirestoreProperty]
        public string Email { get; set; } = default!;

        [FirestoreProperty]
        public string Password { get; set; } = default!;

        [FirestoreProperty]
        public string? Telefono { get; set; }

        [FirestoreProperty]
        public bool Premium { get; set; } = false;

        [FirestoreProperty]
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        [FirestoreProperty]
        public List<string> UsuarioEspacioIds { get; set; } = new();

        [FirestoreProperty]
        public List<string> InvitacionIds { get; set; } = new();
    }
}