using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Convivia.Shared.DTOs
{
    public class CreatePlantillaTareaDto
    {
        [Required]
        public string Nombre { get; set; }

        public DateTime FechaCreacion { get; set; }

        public int karma { get; set; }

        public bool Disponible { get; set; }

        public List<int> DiasRepeticion { get; set; }

        public List<string> TareasId { get; set; }

    }
}