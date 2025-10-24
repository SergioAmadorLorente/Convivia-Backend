using System;
using System.Collections.Generic;

namespace AuthApiDemo.DTOs
{
    public class TareaDto
    {
        public string IdTarea { get; set; }
        public string Nombre { get; set; }
        public DateTime FechaLimite { get; set; }
        public string? Descripcion { get; set; }
        public List<string> UsuarioEspacioIds { get; set; } = new();
        public int Karma { get; set; }
        public byte[]? Foto { get; set; }
        public DateTime? Prorroga { get; set; }
        public bool Estado { get; set; }
        public DateTime? FechaRealizacion { get; set; }
        public string? FacturaId { get; set; } // Referencia a la factura asociada
        public string? PlantillaId { get; set; }
        public List<DayOfWeek> DiasRepeticion { get; set; } = new();

    }
}