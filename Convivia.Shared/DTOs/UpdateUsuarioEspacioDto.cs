using System.Collections.Generic;

namespace Convivia.Shared.DTOs
{
    public class UpdateUsuarioEspacioDto
    {
        public bool? Ausente { get; set; }
        public int? Karma { get; set; }
        public string? Rol { get; set; }
        public string? EspacioId { get; set; }
        public string? UsuarioId { get; set; }
        public List<string>? TareasId { get; set; }
        public string? PermisoId { get; set; }
        public List<string>? FacturasId { get; set; }
    }
}
