using System;
using System.Collections.Generic;

namespace AuthApiDemo.DTOs
{
    public class UpdateTareaDto
    {
        public string? Nombre { get; set; }
        public DateTime? FechaLimite { get; set; }
        public string? Descripcion { get; set; }
        public List<string>? UsuarioEspacioIds { get; set; }
        public int? Karma { get; set; }
        public byte[]? Foto { get; set; }
        public DateTime? Prorroga { get; set; }
        public bool? Estado { get; set; }
        public DateTime? FechaRealizacion { get; set; }
        public string? FacturaId { get; set; } // Referencia a la factura asociada
    }
}
