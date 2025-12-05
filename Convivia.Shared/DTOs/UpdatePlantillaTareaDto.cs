using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Convivia.Shared.DTOs
{
    public class UpdatePlantillaTareaDto
    {
        public string? Nombre { get; set; }

        public DateTime? FechaCreacion { get; set; }

        public int? karma { get; set; }

        public List<int>? DiasRepeticion { get; set; }

    }
}