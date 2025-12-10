using System;
using System.Collections.Generic;

namespace Convivia.Shared.DTOs
{
    public class TareaDto
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public DateTime HoraLimite { get; set; }
        public string? Descripcion { get; set; }
        public List<string> UsuarioEspaciosIds { get; set; }
        public int karma { get; set; }
        public byte[]? Foto { get; set; }
        public DateTime? Prorroga { get; set; }
        public bool Estado { get; set; }
        public DateTime? FechaRealizacion { get; set; }
        public string? FacturaId { get; set; } // Referencia a la factura asociada
        public string? PlantillaId { get; set; }
        public int DiaSemana { get; set; }
        public string? SalaId { get; set; }

    }
}