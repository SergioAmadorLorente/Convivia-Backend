using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convivia.Shared.DTOs
{
    /// <summary>
    /// Datos necesarios para que un usuario se una a un espacio mediante código de acceso.
    /// </summary>
    public class JoinByCodeDto
    {
        /// <summary>
        /// ID del usuario que se unirá al espacio.
        /// </summary>
        public string UsuarioId { get; set; }
    }
}
