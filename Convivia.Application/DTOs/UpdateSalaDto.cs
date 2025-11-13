using Convivia.Domain.Models;
using System.Collections.Generic;

namespace Convivia.Application.DTOs
{
    // DTO para PATCH (actualización parcial) de sala.
    public class UpdateSalaDto
    {
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public string? IdEspacio { get; set; } // id como string
        public List<Reserva>? Reservas { get; set; }
    }

}