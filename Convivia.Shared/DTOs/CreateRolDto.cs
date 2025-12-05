using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convivia.Shared.DTOs
{
    public class CreateRolDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [RegularExpression("^(Usuario|Admin)$", ErrorMessage = "El nombre debe ser: Usuario o Admin")]
        public string Nombre { get; set; }
    }
}
