using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convivia.Shared.DTOs
{
    /// <summary>
    /// Datos para actualizar los permisos individuales de un usuario. Todos los campos son opcionales.
    /// </summary>
    public class UpdatePermisoDto
    {
        /// <summary>
        /// Cambiar el rol base del usuario (opcional).
        /// </summary>
        [EnumDataType(typeof(TipoRol), ErrorMessage = "El rol debe ser v�lido")]
        public TipoRol? Rol { get; set; }
        
        /// <summary>
        /// Actualizar permiso para crear tareas (opcional).
        /// </summary>
        public bool? CrearTarea { get; set; }

        /// <summary>
        /// Actualizar permiso para eliminar tareas (opcional).
        /// </summary>
        public bool? EliminarTarea { get; set; }

        /// <summary>
        /// Actualizar permiso para editar tareas (opcional).
        /// </summary>
        public bool? EditarTarea { get; set; }

        /// <summary>
        /// Actualizar permiso para asignar tareas (opcional).
        /// </summary>
        public bool? AsignarTarea { get; set; }

        /// <summary>
        /// Actualizar permiso para auto-asignarse tareas (opcional).
        /// </summary>
        public bool? AsignarseTarea { get; set; }
        
        /// <summary>
        /// Actualizar permiso para a�adir usuarios (opcional).
        /// </summary>
        public bool? AnadirUsuario { get; set; }

        /// <summary>
        /// Actualizar permiso para eliminar usuarios (opcional).
        /// </summary>
        public bool? EliminarUsuario { get; set; }
        
        /// <summary>
        /// Actualizar permiso para eliminar la residencia (opcional).
        /// </summary>
        public bool? EliminarResidencia { get; set; }
    }
}
