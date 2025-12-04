using System;

namespace Convivia.Shared.DTOs
{
    public class CreatePeticionDto
    {
        public string Mensaje { get; set; } = string.Empty;
        public string IdSolicitante { get; set; } = string.Empty;
        public string IdEspacio { get; set; } = string.Empty;
    }
}
