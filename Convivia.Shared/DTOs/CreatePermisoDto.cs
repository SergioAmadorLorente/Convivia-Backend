using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convivia.Shared.DTOs
{
    public class CreatePermisoDto
    {
        [Required(ErrorMessage = "El rol es requerido")]
        [RegularExpression("^(Usuario|Admin)$", ErrorMessage = "El rol debe ser: Usuario o Admin")]
        public string Rol { get; set; }
        
        // Los permisos se asignan automáticamente según el rol
        // No es necesario enviarlos en la creación
    }
}
