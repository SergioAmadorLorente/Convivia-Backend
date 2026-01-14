using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;

namespace Convivia.Shared.DTOs
{
    public class PlantillaTareaDto
    {
        public string Id { get; set; }

        public string Nombre { get; set; }

        public DateTime FechaCreacion { get; set; }

        public int karma { get; set; }

        public List<int> DiasRepeticion { get; set; }

        public List<string> TareasId { get; set; } = new();

        public string EspacioId { get; set; }

        public string Descripcion { get; set; }

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