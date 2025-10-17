using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AuthApiDemo.DTOs
{
    public class CreatePlantillaTareaDto
    {
        [Required]
        public string Nombre { get; set; }

        public DateTime FechaCreacion { get; set; }

        public int PuntosKarma { get; set; }

        public bool Disponible { get; set; }

        public List<DayOfWeek> DiasRepeticion { get; set; }

    }
}