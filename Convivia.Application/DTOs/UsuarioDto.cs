using System;

namespace Convivia.Aplicacion.DTOs
{
    public class UsuarioDto
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string? Telefono { get; set; }
        public bool Premium { get; set; }
        public DateTime FechaRegistro { get; set; }

        // Opcional: IDs de relaciones para referencias ligeras (evita cargar listas completas)
        public List<string>? UsuarioEspacioIds { get; set; } // IDs de espacios asociados
        public List<string>? InvitacionIds { get; set; } // IDs de invitaciones
    }
}