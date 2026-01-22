using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convivia.Shared.DTOs
{
    /// <summary>
    /// Representa el conjunto de permisos asignados a un usuario en un espacio según su rol.
    /// </summary>
    public class PermisoDto
    {
        /// <summary>
        /// Identificador único del conjunto de permisos.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Rol base asociado a estos permisos.
        /// </summary>
        public TipoRol Rol { get; set; }
        
        /// <summary>
        /// Permiso para crear nuevas tareas.
        /// </summary>
        public bool CrearTarea { get; set; }

        /// <summary>
        /// Permiso para eliminar tareas.
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
        /// Permiso para añadir usuarios al espacio.
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

        public PermisoDto() { }
    }
}
