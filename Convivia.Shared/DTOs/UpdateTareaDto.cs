using System;
using System.Collections.Generic;

namespace Convivia.Shared.DTOs
{
    public class UpdateTareaDto
    {
        public DateTime? FechaRealizacion { get; set; }

        public TimeSpan? Prorroga { get; set; }

        public string? Estado { get; set; }

        public string? UsuarioEspacioId { get; set; }

        public TimeOnly? HoraLimite { get; set; }
    }
}