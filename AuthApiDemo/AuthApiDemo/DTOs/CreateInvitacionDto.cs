using System.ComponentModel.DataAnnotations;

namespace AuthApiDemo.DTOs
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
