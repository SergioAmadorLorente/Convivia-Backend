using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Convivia.Shared.DTOs
{
    public class UsuarioDto
    {

        public string IdUsuario { get;  set; }

        public string Nombre { get; set; }

        public string Email { get; set; }

        public string? Telefono { get; set; }

        public bool Premium { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

      //  [FirestoreProperty("UsuarioEspacios")]
       // public List<UsuarioEspacioId> UsuarioEspacios { get; set; } = new List<UsuarioEspacioId>();

      //  [FirestoreProperty("Invitaciones")]
       // public List<InvitacionId> Invitaciones { get; set; } = new List<InvitacionId>();

        public UsuarioDto() { }
    }
}
