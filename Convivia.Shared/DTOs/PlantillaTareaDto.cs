using System;
using System.Collections.Generic;

namespace Convivia.Shared.DTOs
{
    public class PlantillaTareaDto
    {
        public string PlantillaId { get; set; }

        public string Nombre { get; set; }

        public DateTime FechaCreacion { get; set; }

        public int karma { get; set; }

        public List<int> DiasRepeticion { get; set; }

        public List<string> TareasId { get; set; } = new();

        public string EspacioId { get; set; }

        public string Descripcion { get; set; }

    }
}