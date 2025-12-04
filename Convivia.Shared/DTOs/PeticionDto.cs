using System;

namespace Convivia.Shared.DTOs
{
    public class PeticionDto
    {
        public string Id { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string IdSolicitante { get; set; } = string.Empty;
        public string IdEspacio { get; set; } = string.Empty;

        public PeticionDto() { }
    }
}
