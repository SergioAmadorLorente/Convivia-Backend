using System;
using System.Collections.Generic;

namespace AuthApiDemo.DTOs
{
    public class UpdatePlantillaTareaDto
    {

        public string? Nombre { get; set; }
        public int? PuntosKarma { get; set; }
        public int? Disponible { get; set; }
        public int?[] DiasRepeticion { get; set; }

    }
}
