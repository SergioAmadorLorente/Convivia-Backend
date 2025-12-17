using System;

namespace Convivia.Shared.DTOs
{
    public class InvitacionDto
    {
        public string Id { get; set; } = string.Empty;

        public string UsuarioSolicitanteId { get; set; } = string.Empty;

        public string UsuarioInvitadoId { get; set; } = string.Empty;

        public string EspacioId { get; set; } = string.Empty;

        public string? Mensaje { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        public string Estado { get; set; } = "pendiente";

        public InvitacionDto() { }
    }
}