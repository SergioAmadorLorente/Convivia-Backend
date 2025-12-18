using System.ComponentModel.DataAnnotations;

namespace Convivia.Shared.DTOs
{
    public class UpdateRolDto
    {
        public PermisosRolDto? Permisos { get; set; }
    }
    
    public enum TipoRol
    {
        Admin,
        Usuario,
        Moderador
    }
    
    public class PermisosRolDto
    {
        // Permisos de Tareas
        public bool? CrearTarea { get; set; }
        public bool? EditarTarea { get; set; }
        public bool? EliminarTarea { get; set; }
        public bool? AsignarTarea { get; set; }
        public bool? AsignarseTarea { get; set; }
        
        // Permisos de Usuarios
        public bool? AgregarUsuario { get; set; }
        public bool? EliminarUsuario { get; set; }
        
        // Permisos de Residencia
        public bool? EliminarResidencia { get; set; }
    }
}
