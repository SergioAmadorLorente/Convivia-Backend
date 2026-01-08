using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convivia.Shared.DTOs
{
    public class PermisoDto
    {
        public string Id { get; set; } = string.Empty;
        public TipoRol Rol { get; set; }
        
        // Permisos de Tareas
        public bool CrearTarea { get; set; }
        public bool EliminarTarea { get; set; }
        public bool EditarTarea { get; set; }
        public bool AsignarTarea { get; set; }
        public bool AsignarseTarea { get; set; }
        
        // Permisos de Usuarios
        public bool AñadirUsuario { get; set; }
        public bool EliminarUsuario { get; set; }
        
        // Permisos de Residencia
        public bool EliminarResidencia { get; set; }

        public PermisoDto() { }
    }
}
