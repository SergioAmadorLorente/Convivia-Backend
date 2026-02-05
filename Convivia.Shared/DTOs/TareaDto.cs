using System;
using System.Collections.Generic;

namespace Convivia.Shared.DTOs
{
    /// <summary>
    /// Representa una tarea asignada en un espacio compartido con su estado y detalles.
    /// </summary>
    public class TareaDto
    {
        /// <summary>
        /// Identificador único de la tarea.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Nombre de la tarea.
        /// </summary>
        public string Nombre { get; set; }

        /// <summary>
        /// Hora límite para completar la tarea (opcional).
        /// </summary>
        public TimeOnly? HoraLimite { get; set; }

        /// <summary>
        /// Descripción detallada de la tarea (opcional).
        /// </summary>
        public string? Descripcion { get; set; }

        /// <summary>
        /// Puntos de karma que otorga completar esta tarea. Valores permitidos: 5, 15, 25, 50.
        /// </summary>
        public int karma { get; set; }

        /// <summary>
        /// Estado actual de la tarea (Pendiente o Completada).
        /// </summary>
        public string Estado { get; set; }

        /// <summary>
        /// Fecha y hora en que se realizó la tarea (opcional).
        /// </summary>
        public DateTime? FechaRealizacion { get; set; }

        /// <summary>
        /// ID de la plantilla de tarea desde la que se generó esta instancia (opcional).
        /// </summary>
        public string? PlantillaId { get; set; }

        /// <summary>
        /// Día de la semana asignado (0=Domingo, 1=Lunes, ..., 6=Sábado).
        /// </summary>
        public int DiaSemana { get; set; }

        /// <summary>
        /// Indica si la tarea está vencida (superado el plazo límite sin completar).
        /// </summary>
        public bool Overdue { get; set; }

        /// <summary>
        /// ID del usuario asignado a la tarea (UsuarioEspacioId) (opcional).
        /// </summary>
        public string? UsuarioEspacioId { get; set; }

        /// <summary>
        /// Indica si la tarea es puntual (true) o repetitiva (false).
        /// </summary>
        public bool EsPuntual { get; set; }
    }
}