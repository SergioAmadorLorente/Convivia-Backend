using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Convivia.Application.DTOs
{
    public class CreateTareaDto
    {
        [Required]
        public string Nombre { get; set; }

        [Required]
        public DateTime FechaLimite { get; set; }

        public string? Descripcion { get; set; }

        public List<string> UsuarioEspacioIds { get; set; } = new();

        public int Karma { get; set; } = 10;

        public byte[]? Foto { get; set; }

        public DateTime? Prorroga { get; set; }

        public List<int> DiasRepeticion { get; set; } = new();

    }
}