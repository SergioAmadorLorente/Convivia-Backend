using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Convivia.Shared.DTOs
{
    [FirestoreData]
    public class UsuarioDto
    {

        [FirestoreProperty("Id")]
        public string Id { get;  set; } = Guid.NewGuid().ToString();

        [FirestoreProperty("Nombre")]
        public string Nombre { get; set; }

        [FirestoreProperty("Email")]
        public string Email { get; set; }

        [FirestoreProperty("Telefono")]
        public string? Telefono { get; set; }

        [FirestoreProperty("Premium")]
        public bool Premium { get; set; }

        [FirestoreProperty("FechaRegistro")]
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

      //  [FirestoreProperty("UsuarioEspacios")]
       // public List<UsuarioEspacioId> UsuarioEspacios { get; set; } = new List<UsuarioEspacioId>();

      //  [FirestoreProperty("Invitaciones")]
       // public List<InvitacionId> Invitaciones { get; set; } = new List<InvitacionId>();

        public UsuarioDto() { }
    }
}
