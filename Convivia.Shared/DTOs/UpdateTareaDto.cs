using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Convivia.Application.DTOs
{
    public class UpdateTareaDto
    {
        public string? Nombre { get; set; }

        public DateTime? FechaLimite { get; set; }

        public string? Descripcion { get; set; }

        public List<string>? UsuarioEspacioIds { get; set; } = new();

        public int? PuntosKarma { get; set; } = 10;

        public byte[]? Foto { get; set; }

        public DateTime? Prorroga { get; set; }

        public bool? Estado { get; set; }

        public string? FacturaId { get; set; } // Referencia a la factura asociada

        public string? PlantillaId { get; set; }

    }
}