using Convivia.Domain.Models;
using System.Collections.Generic;


namespace Convivia.Application.DTOs
{
    public class CreateSalaDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string IdEspacio { get; set; } = string.Empty;
        public List<Reserva>? Reservas { get; set; }
    }


    // Alias ligero para compatibilidad con endpoints que usan CreateSalaDto
    public class CreateSalaDto : CreateSalaDTO { }
}