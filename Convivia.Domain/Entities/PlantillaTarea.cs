using Convivia.Shared.DTOs;
using Google.Cloud.Firestore;

namespace Convivia.Domain.Entities
{
    public class PlantillaTarea
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public string TimeZoneId { get; set; } = "Europe/Madrid";

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public int karma { get; set; }

        public List<int> DiasRepeticion { get; set; } = new List<int>();

        public string EspacioId { get; set; }

        public string? FacturaId { get; set; }

        public List<string> TareasId { get; set; }

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

        public PlantillaTarea()
        {
        }

        public PlantillaTarea(string id_PlantillaTarea, string nombre, DateTime fechaCreacion, int karma, int repeticion)
        {
            if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre no puede estar vacío.");
            if (karma < 0) throw new ArgumentException("Los puntos karma deben ser positivos.");

            Id = id_PlantillaTarea;
            Nombre = nombre;
            FechaCreacion = fechaCreacion;
            this.karma = karma;
        }
    }
}