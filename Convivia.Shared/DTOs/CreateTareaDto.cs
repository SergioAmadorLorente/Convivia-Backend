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

        /// <summary>
        /// Lista OPCIONAL de usuarios a asignar a las tareas creadas.
        /// Si se proporciona:
        /// - Para tarea puntual: debe contener exactamente 1 usuario
        /// - Para tarea repetida: puede contener 1 usuario (mismo para todas) o N usuarios (coincidiendo con DiasRepeticion.Count)
        /// Si está vacía o null, las tareas se crean sin usuario asignado (pueden asignarse después via PATCH).
        /// </summary>
        public List<string>? UsuariosAsignacion { get; set; }

        public byte[]? Foto { get; set; }
    }
}