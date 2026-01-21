using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;

namespace Convivia.Shared.DTOs
{
    /// <summary>
    /// Representa una plantilla de tarea que define cómo se generan las instancias de tareas repetidas o puntuales.
    /// </summary>
    public class PlantillaTareaDto
    {
        /// <summary>
        /// Identificador único de la plantilla de tarea.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Nombre de la plantilla de tarea.
        /// </summary>
        public string Nombre { get; set; }

        /// <summary>
        /// Fecha y hora de creación de la plantilla en formato UTC.
        /// </summary>
        public DateTime FechaCreacion { get; set; }

        /// <summary>
        /// Puntos de karma que otorgan las tareas generadas desde esta plantilla. Valores permitidos: 5, 15, 25, 50.
        /// </summary>
        public int karma { get; set; }

        /// <summary>
        /// Días de repetición semanal (0=Domingo, 1=Lunes, ..., 6=Sábado). Si está vacío, es una tarea puntual.
        /// </summary>
        public List<int> DiasRepeticion { get; set; }

        /// <summary>
        /// Lista de IDs de las tareas generadas desde esta plantilla.
        /// </summary>
        public List<string> TareasId { get; set; } = new();

        /// <summary>
        /// ID del espacio al que pertenece la plantilla.
        /// </summary>
        public string EspacioId { get; set; }

        /// <summary>
        /// Descripción detallada de la plantilla de tarea.
        /// </summary>
        public string Descripcion { get; set; }

        /// <summary>
        /// ID de factura asociada a las tareas generadas (opcional).
        /// </summary>
        public string? FacturaId { get; set; }

        /// <summary>
        /// Fecha límite para tareas puntuales o límite de repetición para tareas repetidas.
        /// Si está seteada, las tareas repetidas se ejecutan hasta esa fecha.
        /// Si no está seteada, las tareas repetidas se ejecutan indefinidamente.
        /// </summary>
        public DateOnly? FechaLimite { get; set; }

        /// <summary>
        /// Indica si la tarea es puntual (sin repetición) o repetida.
        /// true = tarea puntual (DiasRepeticion vacío)
        /// false = tarea repetida (DiasRepeticion con valores)
        /// </summary>
        public bool EsPuntual { get; set; }
    }
}