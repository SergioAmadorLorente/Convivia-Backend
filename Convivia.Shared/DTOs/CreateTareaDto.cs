using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Convivia.Shared.DTOs
{
    public class CreateTareaDto
    {
        [Required]
        public string Nombre { get; set; }

        [Required]
        public DateTime HoraLimite { get; set; }

        public string? Descripcion { get; set; }

        [Required]
        public List<string> UsuarioEspaciosIds { get; set; }

        [Required]
        public int karma { get; set; }

        public byte[]? Foto { get; set; }

        public DateTime? Prorroga { get; set; }

        [Required]
        public List<int> DiasRepeticion { get; set; } = new();

    }
}