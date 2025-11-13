using System;
using System.Collections.Generic;

namespace Convivia.Application.DTOs
{
    public class PlantillaTareaDto
    {
        public string PlantillaId { get; set; }

        public string Nombre { get; set; }

        public DateTime FechaCreacion { get; set; }

        public int PuntosKarma { get; set; }

        public bool Disponible { get; set; }

        public List<DayOfWeek> DiasRepeticion { get; set; }

    }
}