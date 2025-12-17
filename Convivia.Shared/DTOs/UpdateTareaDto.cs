using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Convivia.Shared.DTOs
{
    public class UpdateTareaDto
    {

        public List<string>? UsuarioEspaciosIds { get; set; }

        public DateTime? FechaRealizacion { get; set; }

        public byte[]? Foto { get; set; }

        public DateTime? Prorroga { get; set; }

        public bool? Disponible { get; set; }

        public bool? Completada { get; set; }

        public string? SalaId { get; set; }

    }
}