using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convivia.Shared.DTOs
{
    public class UpdatePermisoDto
    {
        [EnumDataType(typeof(TipoRol), ErrorMessage = "El rol debe ser válido")]
        public TipoRol? Rol { get; set; }
        
        public bool? CrearTarea { get; set; }
        public bool? EliminarTarea { get; set; }
        public bool? EditarTarea { get; set; }
        public bool? AñadirUsuario { get; set; }
        public bool? EliminarUsuario { get; set; }
        public bool? AsignarTarea { get; set; }
        public bool? AsignarseTarea { get; set; }
    }
}
