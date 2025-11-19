using System.ComponentModel.DataAnnotations;

namespace Convivia.Aplicacion.DTOs
{
    public class CreateUsuarioDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres.")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "El email debe tener un formato válido.")]
        [StringLength(150, ErrorMessage = "El email no puede exceder 150 caracteres.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(255, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        public string Password { get; set; } // En producción, hashea antes de guardar (ej. BCrypt.HashPassword)

        [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres.")]
        public string? Telefono { get; set; } 

        public bool Premium { get; set; } = false; 
    }
}