using AuthApiDemo.Models;
using System.Collections.Generic;

namespace AuthApiDemo.DTOs
{
    public class SalaDto
    {
        public string IdSala { get; set; } = default!;
        public string Nombre { get; set; } = default!;
        public string? Descripcion { get; set; }
        public string? IdEspacio { get; set; } // id como string para Swagger/JSON
        public List<Reserva>? Reservas { get; set; }
    }
}