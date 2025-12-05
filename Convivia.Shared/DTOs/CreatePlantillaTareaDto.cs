using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Convivia.Shared.DTOs
{
    public class CreatePlantillaTareaDto
    {
        [Required]
        public string Nombre { get; set; }

        public string? Descripcion { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public int karma { get; set; }

        public List<int> DiasRepeticion { get; set; } = new();

        public List<string> TareasId { get; set; } = new();

    }
}