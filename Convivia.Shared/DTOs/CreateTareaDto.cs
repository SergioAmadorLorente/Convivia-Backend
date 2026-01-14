using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Convivia.Shared.DTOs
{
    public class CreateTareaDto
    {
        [Required]
        public string Nombre { get; set; }

        public string? Descripcion { get; set; }

        [Required]
        [Range(5, 50, ErrorMessage = "Karma debe ser 5, 15, 25 o 50")]
        public int karma { get; set; }

        /// <summary>
        /// Días de repetición semanal enviados desde el front.
        /// Formato del cliente: 0=Lunes, 1=Martes, 2=Miércoles, 3=Jueves, 4=Viernes, 5=Sábado, 6=Domingo
        /// Se convierten internamente a: 0=Domingo, 1=Lunes, 2=Martes, 3=Miércoles, 4=Jueves, 5=Viernes, 6=Sábado
        /// </summary>
        public List<int> DiasRepeticion { get; set; } = new();

        [Required]
        public TimeOnly HoraLimite { get; set; }

        /// <summary>
        /// Fecha límite (solo fecha, sin hora).
        /// Formato: YYYY-MM-DD (e.g., "2025-12-30")
        /// 
        /// OBLIGATORIA: Si DiasRepeticion está vacío (tarea puntual)
        /// OPCIONAL: Si DiasRepeticion tiene valores (tarea repetida)
        /// 
        /// Comportamiento:
        /// - Tarea puntual (sin DiasRepeticion): Se ejecuta una sola vez en FechaLimite a HoraLimite
        /// - Tarea repetida con FechaLimite: Se repite en los DiasRepeticion hasta la FechaLimite
        /// - Tarea repetida sin FechaLimite: Se repite indefinidamente en los DiasRepeticion
        /// </summary>
        public DateOnly? FechaLimite { get; set; }

        public List<string>? UsuariosAsignacion { get; set; }
    }
}