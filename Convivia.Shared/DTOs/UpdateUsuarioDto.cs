using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convivia.Shared.DTOs
{
    /// <summary>
    /// Datos para actualizar la informacion de un usuario existente. Todos los campos son opcionales.
    /// </summary>
    public class UpdateUsuarioDto
    {
        /// <summary>
        /// Nuevo nombre del usuario (opcional).
        /// </summary>
        public string? Nombre { get; set; }

        /// <summary>
        /// Nuevo correo electronico (opcional). Debe ser unico.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Nueva contrasena (opcional). Se almacenara de forma segura.
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Nuevo numero de telefono (opcional).
        /// </summary>
        public string? Telefono { get; set; }

        /// <summary>
        /// Actualizar el estado Premium del usuario (opcional).
        /// </summary>
        public bool? Premium { get; set; }

        /// <summary>
        /// URL publica de la foto de perfil del usuario (opcional).
        /// </summary>
        public string? FotoUrl { get; set; }
    }
}
