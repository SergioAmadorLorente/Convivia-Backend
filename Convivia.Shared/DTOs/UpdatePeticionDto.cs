using System;

namespace Convivia.Shared.DTOs
{
    public class UpdatePeticionDto
    {
        public string? Mensaje { get; set; }
        public string? Estado { get; set; }
        public string? IdSolicitante { get; set; }
        public string? IdEspacio { get; set; }
    }
}
