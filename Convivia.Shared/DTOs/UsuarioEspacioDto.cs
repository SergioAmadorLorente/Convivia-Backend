using System;
using System.Collections.Generic;

namespace Convivia.Shared.DTOs
{
    /// <summary>
    /// Representa la relación entre un usuario y un espacio compartido, incluyendo su rol, karma y permisos.
    /// </summary>
    public class UsuarioEspacioDto
    {
        /// <summary>
        /// Identificador único de la relación usuario-espacio.
        /// </summary>
        public string Id { get; set; } = string.Empty;
        
        /// <summary>
        /// Indica si el usuario está ausente temporalmente del espacio.
        /// </summary>
        public bool Ausente { get; set; }
        
        /// <summary>
        /// Puntos de karma acumulados por el usuario en este espacio.
        /// </summary>
        public int Karma { get; set; }
        
        /// <summary>
        /// Rol asignado al usuario en el espacio (Admin, Usuario, Moderador).
        /// </summary>
        public string Rol { get; set; } = string.Empty;
        
        /// <summary>
        /// Identificador del espacio al que pertenece.
        /// </summary>
        public string EspacioId { get; set; } = string.Empty;
        
        /// <summary>
        /// Identificador del usuario.
        /// </summary>
        public string UsuarioId { get; set; } = string.Empty;
        
        /// <summary>
        /// Lista de IDs de tareas asignadas al usuario en este espacio.
        /// </summary>
        public List<string> TareasId { get; set; } = new();
        
        /// <summary>
        /// Identificador del conjunto de permisos del usuario.
        /// </summary>
        public string PermisoId { get; set; } = string.Empty;
        
        public List<string> FacturasId { get; set; } = new();

        public UsuarioEspacioDto() { }
    }
}
