using System.ComponentModel.DataAnnotations;

namespace AuthApiDemo.DTOs
{
    public class CreateInvitacionDto
    {
        [Required]
        public string UsuarioSolicitanteId { get; set; } = default;

        [Required]
        public string UsuariosInvitadoId { get; set; } = default;

        [Required]
        public string EspacioId { get; set; } = default;
        
        // mensaje que envia el solicitante
        public string? Mensaje {  get; set; }
    }
}
