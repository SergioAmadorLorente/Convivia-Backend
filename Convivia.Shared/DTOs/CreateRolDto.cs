using System.ComponentModel.DataAnnotations;

namespace Convivia.Shared.DTOs
{
    /// <summary>
    /// Datos necesarios para crear un nuevo rol. Los permisos se asignan automáticamente según el tipo de rol.
    /// </summary>
    public class CreateRolDto
    {
        /// <summary>
        /// Tipo de rol a crear (Admin, Usuario, Moderador). Los permisos se configuran automáticamente.
        /// </summary>
        [Required(ErrorMessage = "El nombre del rol es requerido")]
        [EnumDataType(typeof(TipoRol), ErrorMessage = "El rol debe ser válido")]
        public TipoRol Nombre { get; set; } 
    }

}
