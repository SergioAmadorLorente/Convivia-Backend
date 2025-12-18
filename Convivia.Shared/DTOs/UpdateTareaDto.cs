using System;
using System.Collections.Generic;

namespace Convivia.Shared.DTOs
{
    public class UpdateTareaDto
    {
        public DateTime? FechaRealizacion { get; set; }

        public byte[]? Foto { get; set; }

        public DateTime? Prorroga { get; set; }

        // Estado como string - será parseado en el service
        public string? Estado { get; set; }

        /// <summary>
        /// ID del usuario asignado a la tarea (1 solo) - OPCIONAL.
        /// </summary>
        public string? UsuarioEspacioId { get; set; }
    }
}