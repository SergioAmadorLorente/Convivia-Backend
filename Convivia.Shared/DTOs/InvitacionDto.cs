using System;

namespace Convivia.Shared.DTOs
{
    public class InvitacionDto
    {
        public string IdInvitacion { get; set; }

        public string UsuarioSolicitanteId { get; set; } 

        public string UsuarioInvitadoId { get; set; } 

        public 6string EspacioId { get; set; } 

        public string? Mensaje { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        public string Estado { get; set; } = "pendiente";

        public InvitacionDto() { }
    }
}