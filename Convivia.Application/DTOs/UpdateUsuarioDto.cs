using System.ComponentModel.DataAnnotations;

namespace Convivia.Aplicacion.DTOs
{
    public class UpdateUsuarioDto
    {
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres.")]
        public string? Nombre { get; set; } // Opcional para updates parciales

        [EmailAddress(ErrorMessage = "El email debe tener un formato válido.")]
        [StringLength(150, ErrorMessage = "El email no puede exceder 150 caracteres.")]
        public string? Email { get; set; } // Opcional

        [StringLength(255, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        public string? Password { get; set; } // Opcional; hashea si se actualiza

        [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres.")]
        public string? Telefono { get; set; } // Opcional

        public bool? Premium { get; set; } // Opcional (nullable para no cambiar si no se envía)
    }
}