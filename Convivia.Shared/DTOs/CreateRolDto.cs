using System.ComponentModel.DataAnnotations;

namespace Convivia.Shared.DTOs
{
    public class CreateRolDto
    {
        [Required(ErrorMessage = "El nombre del rol es requerido")]
        [EnumDataType(typeof(TipoRol), ErrorMessage = "El rol debe ser válido")]
        public TipoRol Nombre { get; set; } 
    }

}
