using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convivia.Shared.DTOs
{
    /// <summary>
    /// Datos necesarios para registrar un nuevo usuario en el sistema.
    /// </summary>
    public class CreateUsuarioDto
    {
        /// <summary>
        /// Identificador único del usuario (opcional, generado automáticamente si no se proporciona).
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Nombre completo del usuario.
        /// </summary>
        public string Nombre { get; set; }

        /// <summary>
        /// Correo electrónico del usuario. Debe ser único en el sistema.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Contraseña del usuario
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Número de teléfono del usuario (opcional).
        /// </summary>
        public string? Telefono { get; set; }
    }
}
