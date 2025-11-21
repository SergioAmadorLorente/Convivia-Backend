using System;
using System.Collections.Generic;

namespace Convivia.Application.DTOs
{
    public class UpdatePlantillaTareaDto
    {

        public string? Nombre { get; set; }
        public int? PuntosKarma { get; set; }
        public int? Disponible { get; set; }
        public int?[] DiasRepeticion { get; set; }

    }
}
