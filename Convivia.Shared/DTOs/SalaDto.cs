using System;

namespace Convivia.Shared.DTOs
{
    public class SalaDto
    {
        public string Id { get; set; } = string.Empty;
        
        public string Nombre { get; set; } = string.Empty;
        
        public string? Descripcion { get; set; }
        
        public string IdEspacio { get; set; } = string.Empty;

        public SalaDto() { }
    }
}
