using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Convivia.Shared.DTOs
{
    /// <summary>
    /// Representa un rol con sus permisos asociados en un espacio compartido.
    /// </summary>
    public class RolDto
    {
        /// <summary>
        /// Identificador único del rol.
        /// </summary>
        public string Id { get; set; } = string.Empty;
        
        /// <summary>
        /// Tipo de rol (Admin, Usuario, Moderador).
        /// </summary>
        [Required]
        public TipoRol Nombre { get; set; }
        
        /// <summary>
        /// Permiso para crear nuevas tareas.
        /// </summary>
        public bool CrearTarea { get; set; }

        /// <summary>
        /// Permiso para eliminar tareas existentes.
        /// </summary>
        public bool EliminarTarea { get; set; }

        /// <summary>
        /// Permiso para editar tareas.
        /// </summary>
        public bool EditarTarea { get; set; }

        /// <summary>
        /// Permiso para asignar tareas a otros usuarios.
        /// </summary>
        public bool AsignarTarea { get; set; }

        /// <summary>
        /// Permiso para auto-asignarse tareas.
        /// </summary>
        public bool AsignarseTarea { get; set; }
        
        /// <summary>
        /// Permiso para añadir nuevos usuarios al espacio.
        /// </summary>
        public bool AñadirUsuario { get; set; }

        /// <summary>
        /// Permiso para eliminar usuarios del espacio.
        /// </summary>
        public bool EliminarUsuario { get; set; }
        
        /// <summary>
        /// Permiso para eliminar el espacio/residencia completo.
        /// </summary>
        public bool EliminarResidencia { get; set; }

        public RolDto() { }
    }
}
