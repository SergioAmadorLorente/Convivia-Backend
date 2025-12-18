using System;
using System.Collections.Generic;

namespace Convivia.Shared.DTOs
{
    public class UpdateTareaDto
    {
        public DateTime? FechaRealizacion { get; set; }

        public byte[]? Foto { get; set; }

        public DateTime? Prorroga { get; set; }

        public bool? Disponible { get; set; }

        public bool? Completada { get; set; }

        /// <summary>
        /// ID del usuario asignado a la tarea (1 solo).
        /// </summary>
        public string? UsuarioEspacioId { get; set; }
    }
}