using System.ComponentModel.DataAnnotations;

namespace Convivia.Shared.DTOs
{
    /// <summary>
    /// Datos para actualizar los permisos de un rol existente.
    /// </summary>
    public class UpdateRolDto
    {
        /// <summary>
        /// Conjunto de permisos a actualizar (opcional).
        /// </summary>
        public PermisosRolDto? Permisos { get; set; }
    }
    
    /// <summary>
    /// Enumeración de los tipos de rol disponibles en el sistema.
    /// </summary>
    public enum TipoRol
    {
        /// <summary>
        /// Administrador con acceso completo.
        /// </summary>
        Admin,

        /// <summary>
        /// Usuario estándar con permisos limitados.
        /// </summary>
        Usuario,

        /// <summary>
        /// Moderador con permisos intermedios.
        /// </summary>
        Moderador
    }
    
    /// <summary>
    /// Define los permisos individuales que se pueden actualizar para un rol.
    /// </summary>
    public class PermisosRolDto
    {
        /// <summary>
        /// Permiso para crear tareas (opcional).
        /// </summary>
        public bool? CrearTarea { get; set; }

        /// <summary>
        /// Permiso para editar tareas (opcional).
        /// </summary>
        public bool? EditarTarea { get; set; }

        /// <summary>
        /// Permiso para eliminar tareas (opcional).
        /// </summary>
        public bool? EliminarTarea { get; set; }

        /// <summary>
        /// Permiso para asignar tareas a otros usuarios (opcional).
        /// </summary>
        public bool? AsignarTarea { get; set; }

        /// <summary>
        /// Permiso para auto-asignarse tareas (opcional).
        /// </summary>
        public bool? AsignarseTarea { get; set; }
        
        /// <summary>
        /// Permiso para agregar usuarios al espacio (opcional).
        /// </summary>
        public bool? AgregarUsuario { get; set; }

        /// <summary>
        /// Permiso para eliminar usuarios del espacio (opcional).
        /// </summary>
        public bool? EliminarUsuario { get; set; }
        
        /// <summary>
        /// Permiso para eliminar el espacio completo (opcional).
        /// </summary>
        public bool? EliminarResidencia { get; set; }
    }
}
