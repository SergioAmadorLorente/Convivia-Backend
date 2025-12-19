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
        /// Días de la semana (0=Domingo, 6=Sábado) para tareas repetidas.
        /// Si está vacío, es una tarea puntual.
        /// </summary>
        public List<int> DiasRepeticion { get; set; } = new();

        [Required]
        public TimeOnly HoraLimite { get; set; }

        /// <summary>
        /// Fecha límite para tarea puntual O fecha fin para tarea repetida.
        /// Opcional en creación (puede omitirse), validaciones en servicio.
        /// </summary>
        public DateTime? FechaLimite { get; set; }

        /// <summary>
        /// Fecha fin opcional para tarea repetida.
        /// </summary>
        public DateTime? FechaFin { get; set; }

        /// <summary>        /// Lista obligatoria de usuarios a asignar a las tareas creadas.
        /// Si solo contiene 1 elemento, ese usuario se asigna a todas las tareas.
        /// Si contiene más de 1 elemento y la tarea es repetida, el número de usuarios debe
        /// coincidir con el número de tareas creadas (DiasRepeticion.Count).
        /// El orden de la lista determina la correspondencia usuario->tarea.
        /// </summary>
        [Required]
        public List<string> UsuariosAsignacion { get; set; } = new();

        public byte[]? Foto { get; set; }
    }
}