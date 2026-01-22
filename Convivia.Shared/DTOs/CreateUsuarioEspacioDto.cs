using System.Collections.Generic;

namespace Convivia.Shared.DTOs
{
    /// <summary>
    /// Datos necesarios para crear una nueva relación entre un usuario y un espacio.
    /// </summary>
    public class CreateUsuarioEspacioDto
    {
        /// <summary>
        /// Estado de ausencia inicial del usuario en el espacio.
        /// </summary>
        public bool Ausente { get; set; }

        /// <summary>
        /// Karma inicial del usuario en el espacio.
        /// </summary>
        public int Karma { get; set; }

        /// <summary>
        /// Rol asignado al usuario (Admin, Usuario, Moderador).
        /// </summary>
        public string Rol { get; set; } = string.Empty;

        /// <summary>
        /// ID del espacio al que se unirá el usuario.
        /// </summary>
        public string EspacioId { get; set; } = string.Empty;

        /// <summary>
        /// ID del usuario que se unirá al espacio.
        /// </summary>
        public string UsuarioId { get; set; } = string.Empty;

        /// <summary>
        /// Lista inicial de IDs de tareas asignadas.
        /// </summary>
        public List<string> TareasId { get; set; } = new();

        /// <summary>
        /// ID del conjunto de permisos asignado al usuario.
        /// </summary>
        public string PermisoId { get; set; } = string.Empty;
        public List<string> FacturasId { get; set; } = new();
    }
}
