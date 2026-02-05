using System;
using System.Collections.Generic;

namespace Convivia.Shared.DTOs
{
    /// <summary>
    /// Datos para actualizar una tarea existente. Todos los campos son opcionales.
    /// </summary>
    public class UpdateTareaDto
    {
        /// <summary>
        /// Nueva fecha y hora de realización de la tarea (opcional).
        /// </summary>
        public DateTime? FechaRealizacion { get; set; }

        /// <summary>
        /// Nuevo estado de la tarea (opcional).
        /// </summary>
        public string? Estado { get; set; }

        /// <summary>
        /// Nuevo usuario asignado a la tarea (UsuarioEspacioId) (opcional).
        /// </summary>
        public string? UsuarioEspacioId { get; set; }

        /// <summary>
        /// Nueva hora límite para completar la tarea (opcional).
        /// </summary>
        public TimeOnly? HoraLimite { get; set; }
    }
}