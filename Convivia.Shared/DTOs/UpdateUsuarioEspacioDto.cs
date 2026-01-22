using System.Collections.Generic;

namespace Convivia.Shared.DTOs
{
    /// <summary>
    /// Datos para actualizar la relación usuario-espacio. Todos los campos son opcionales.
    /// </summary>
    public class UpdateUsuarioEspacioDto
    {
        /// <summary>
        /// Actualizar el estado de ausencia del usuario (opcional).
        /// </summary>
        public bool? Ausente { get; set; }

        /// <summary>
        /// Actualizar los puntos de karma del usuario (opcional).
        /// </summary>
        public int? Karma { get; set; }

        /// <summary>
        /// Actualizar el rol del usuario (opcional).
        /// </summary>
        public string? Rol { get; set; }

        /// <summary>
        /// Actualizar el ID del espacio (opcional).
        /// </summary>
        public string? EspacioId { get; set; }

        /// <summary>
        /// Actualizar el ID del usuario (opcional).
        /// </summary>
        public string? UsuarioId { get; set; }

        /// <summary>
        /// Actualizar la lista de IDs de tareas asignadas (opcional).
        /// </summary>
        public List<string>? TareasId { get; set; }

        /// <summary>
        /// Actualizar el ID del conjunto de permisos (opcional).
        /// </summary>
        public string? PermisoId { get; set; }
        public List<string>? FacturasId { get; set; }
    }
}
