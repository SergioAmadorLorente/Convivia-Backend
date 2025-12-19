using Google.Cloud.Firestore;

namespace Convivia.Domain.Entities
{
    public class PlantillaTarea
    {
        public string PlantillaId { get; set; } = Guid.NewGuid().ToString("N");

        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public string TimeZoneId { get; set; } = "Europe/Madrid";

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public int karma { get; set; }

        public List<int> DiasRepeticion { get; set; } = new List<int>();

        public string EspacioId { get; set; }

        public string? FacturaId { get; set; }

        public List<string> TareasId { get; set; }

        // HoraLimite removed from plantilla: hora se almacena por tarea individual

        public int? GracePeriodMinutes { get; set; } = 30;

        public DateOnly? StartDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
        public DateOnly? EndDate { get; set; } = null;

        public PlantillaTarea()
        {
        }

        public PlantillaTarea(string id_PlantillaTarea, string nombre, DateTime fechaCreacion, int karma, int repeticion)
        {
            if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre no puede estar vacío.");
            if (karma < 0) throw new ArgumentException("Los puntos karma deben ser positivos.");

            PlantillaId = id_PlantillaTarea;
            Nombre = nombre;
            FechaCreacion = fechaCreacion;
            this.karma = karma;
        }
    }
}
