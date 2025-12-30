using System;
using System.Collections.Generic;

namespace Convivia.Shared.DTOs
{
    public class TareaDto
    {
        public string Id { get; set; }

        public string Nombre { get; set; }

        public TimeOnly? HoraLimite { get; set; }

        public string? Descripcion { get; set; }

        public int karma { get; set; }

        public TimeSpan? Prorroga { get; set; }

        public string Estado { get; set; }

        public DateTime? FechaRealizacion { get; set; }

        public string? PlantillaId { get; set; }

        public int DiaSemana { get; set; }

        public bool Overdue { get; set; }

        public string? UsuarioEspacioId { get; set; }

        public DateOnly? FechaLimite { get; set; }

        public bool EsPuntual { get; set; }
    }
}