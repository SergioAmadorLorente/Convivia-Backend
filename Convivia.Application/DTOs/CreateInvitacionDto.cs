using System.ComponentModel.DataAnnotations;

namespace Convivia.Application.DTOs
{
    public class CreateInvitacionDto
    {
        [Required]
        public string UsuarioSolicitanteId { get; set; } = default!;

        public string? UsuarioInvitadoId { get; set; } = default;

        [Required]
        public string EspacioId { get; set; } = default!;

        public string? Mensaje { get; set; }
    }
}
