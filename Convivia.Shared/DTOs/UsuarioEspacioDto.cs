using System;
using System.Collections.Generic;

namespace Convivia.Shared.DTOs
{
    public class UsuarioEspacioDto
    {
        public string Id { get; set; } = string.Empty;
        
        public bool Ausente { get; set; }
        
        public int Karma { get; set; }
        
        public string Rol { get; set; } = string.Empty;
        
        public string EspacioId { get; set; } = string.Empty;
        
        public string UsuarioId { get; set; } = string.Empty;
        
        public List<string> TareasId { get; set; } = new();
        
        public string PermisoId { get; set; } = string.Empty;
        
        public List<string> FacturasId { get; set; } = new();

        public UsuarioEspacioDto() { }
    }
}
