using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convivia.Shared.DTOs
{
    /// <summary>
    /// Datos para actualizar la información de un usuario existente. Todos los campos son opcionales.
    /// </summary>
    public class UpdateUsuarioDto
    {
        /// <summary>
        /// Nuevo nombre del usuario (opcional).
        /// </summary>
        public string? Nombre { get; set; }

        /// <summary>
        /// Nuevo correo electrónico (opcional). Debe ser único.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Nueva contraseña (opcional). Se almacenará de forma segura.
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Nuevo número de teléfono (opcional).
        /// </summary>
        public string? Telefono { get; set; }

        /// <summary>
        /// Actualizar el estado Premium del usuario (opcional).
        /// </summary>
        public bool? Premium { get; set; }
    }
}
