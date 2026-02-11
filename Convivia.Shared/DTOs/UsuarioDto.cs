using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Convivia.Shared.DTOs
{
    /// <summary>
    /// Representa un usuario del sistema con su información personal y estado.
    /// </summary>
    public class UsuarioDto
    {
        /// <summary>
        /// Identificador único del usuario.
        /// </summary>
        public string Id { get;  set; }

        /// <summary>
        /// Nombre completo del usuario.
        /// </summary>
        public string Nombre { get; set; }

        /// <summary>
        /// Correo electrónico del usuario.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Número de teléfono del usuario (opcional).
        /// </summary>
        public string? Telefono { get; set; }

        /// <summary>
        /// Indica si el usuario tiene una cuenta Premium con funcionalidades adicionales.
        /// </summary>
        public bool Premium { get; set; }

        /// <summary>
        /// Fecha y hora de registro del usuario en formato UTC.
        /// </summary>
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

      //  [FirestoreProperty("UsuarioEspacios")]
       // public List<UsuarioId> UsuarioEspacios { get; set; } = new List<UsuarioId>();

      //  [FirestoreProperty("Invitaciones")]
       // public List<InvitacionId> Invitaciones { get; set; } = new List<InvitacionId>();

        public UsuarioDto() { }
    }
}
