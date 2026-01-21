using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convivia.Shared.DTOs
{
    /// <summary>
    /// Datos necesarios para crear un nuevo conjunto de permisos. Los permisos se asignan automáticamente según el rol.
    /// </summary>
    public class CreatePermisoDto
    {
        /// <summary>
        /// Tipo de rol base del que se derivan los permisos (Admin, Usuario, Moderador).
        /// </summary>
        [Required(ErrorMessage = "El rol es requerido")]
        [EnumDataType(typeof(TipoRol), ErrorMessage = "El rol debe ser válido")]
        public TipoRol Rol { get; set; }
        
        // Los permisos se asignan automáticamente según el rol
        // No es necesario enviarlos en la creación
    }
}
