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

        public byte[]? Foto { get; set; }

        public TimeSpan? Prorroga { get; set; }

        // Reemplazamos Completada por Estado (como string en DTO)
        public string Estado { get; set; }

        public DateTime? FechaRealizacion { get; set; }

        public string? FacturaId { get; set; }

        public string? PlantillaId { get; set; }

        public int DiaSemana { get; set; }

        /// <summary>
        /// ¿Está vencida la tarea?
        /// </summary>
        public bool Overdue { get; set; }

        /// <summary>
        /// ID del usuario asignado (1 solo).
        /// </summary>
        public string? UsuarioEspacioId { get; set; }

        /// <summary>
        /// Fecha límite de la tarea.
        /// </summary>
        public DateTime? FechaLimite { get; set; }

        /// <summary>
        /// ¿Es una tarea puntual o repetida?
        /// True = puntual (sin días de repetición).
        /// False = repetida.
        /// </summary>
        public bool EsPuntual { get; set; }
    }
}