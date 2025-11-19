using System.ComponentModel.DataAnnotations;

namespace Convivia.Aplicacion.DTOs
{
    public class AddUsuariosTareaDto
    {
        [Required(ErrorMessage = "El ID del usuario es obligatorio.")]
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El ID de la tarea es obligatorio.")]
        public int IdTarea { get; set; }

        // Propiedades opcionales derivadas de Usuario (si necesitas más datos en la respuesta)
        [StringLength(100)]
        public string NombreUsuario { get; set; } // Para mostrar en UI, ej. "Sergio Amador"

        [EmailAddress]
        [StringLength(150)]
        public string EmailUsuario { get; set; } // Para notificaciones o logs

        // Fecha de asignación (opcional, para auditoría)
        public DateTime? FechaAsignacion { get; set; } = DateTime.UtcNow;
    }
}