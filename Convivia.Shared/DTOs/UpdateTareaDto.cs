using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Convivia.Shared.DTOs
{
    public class UpdateTareaDto
    {
        public string? Nombre { get; set; }

        public TimeOnly? HoraLimite { get; set; }

        public string? Descripcion { get; set; }

        public List<string>? UsuarioEspaciosIds { get; set; }

        public DateTime? FechaRealizacion { get; set; }

        public int? karma { get; set; }

        public byte[]? Foto { get; set; }

        public DateTime? Prorroga { get; set; }

        public bool? Disponible { get; set; }

        public bool? Completada { get; set; }

        public string? FacturaId { get; set; } // Referencia a la factura asociada

        public string? SalaId { get; set; }

    }
}