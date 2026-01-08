using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Convivia.Shared.DTOs
{
    public class CreateTareaDto
    {
        [Required]
        public string Nombre { get; set; }

        public string? Descripcion { get; set; }

        [Required]
        [Range(5, 50, ErrorMessage = "Karma debe ser 5, 15, 25 o 50")]
        public int karma { get; set; }

        public List<int> DiasRepeticion { get; set; } = new();

        [Required]
        public TimeOnly HoraLimite { get; set; }

        public DateTime? FechaLimite { get; set; }

        public DateTime? FechaFin { get; set; }

        public List<string>? UsuariosAsignacion { get; set; }
    }
}