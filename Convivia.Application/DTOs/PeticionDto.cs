using System.ComponentModel.DataAnnotations;

namespace Convivia.Application.DTOs
{
    /// <summary>
    /// DTO para crear una nueva petición
    /// </summary>
    public class CreatePeticionDto
    {
        /// <summary>
        /// ID opcional. Si no se proporciona, se genera automáticamente.
        /// </summary>
        public string? Id { get; set; }

        [Required(ErrorMessage = "El mensaje es obligatorio")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "El mensaje debe tener entre 10 y 500 caracteres")]
        public string Mensaje { get; set; } = string.Empty;

        [Required(ErrorMessage = "El ID del solicitante es obligatorio")]
        public string IdSolicitante { get; set; } = string.Empty;

        [Required(ErrorMessage = "El ID del espacio es obligatorio")]
        public string IdEspacio { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para actualizar parcialmente una petición
    /// </summary>
    public class UpdatePeticionDto
    {
        [StringLength(500, MinimumLength = 10, ErrorMessage = "El mensaje debe tener entre 10 y 500 caracteres")]
        public string? Mensaje { get; set; }

        [RegularExpression("^(pendiente|aceptada|rechazada|cancelada)$", 
            ErrorMessage = "Estado debe ser: pendiente, aceptada, rechazada o cancelada")]
        public string? Estado { get; set; }

        public string? IdSolicitante { get; set; }

        public string? IdEspacio { get; set; }
    }

    /// <summary>
    /// DTO de respuesta para peticiones
    /// </summary>
    public class PeticionDto
    {
        public string Id { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string IdSolicitante { get; set; } = string.Empty;
        public string IdEspacio { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para cambiar el estado de una petición
    /// </summary>
    public class CambiarEstadoPeticionDto
    {
        [Required(ErrorMessage = "La acción es obligatoria")]
        [RegularExpression("^(aceptar|rechazar|cancelar)$", 
            ErrorMessage = "Acción debe ser: aceptar, rechazar o cancelar")]
        public string Accion { get; set; } = string.Empty;
    }
}
