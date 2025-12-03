namespace Convivia.Domain.Entities
{
    public class PlantillaTarea
    {
        public string PlantillaId { get; set; } = Guid.NewGuid().ToString("N");

        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public int PuntosKarma { get; set; }

        public bool Estado { get; set; }

        public List<int> DiasRepeticion { get; set; } = new List<int>();

        // public List<Tarea> tareas { get; set; } = new List<Tarea>();

        public PlantillaTarea()
        {
        }

        public PlantillaTarea(string id_PlantillaTarea, string nombre, DateTime fechaCreacion, int puntosKarma, bool estado, int repeticion)
        {
            if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre no puede estar vacío.");
            if (puntosKarma < 0) throw new ArgumentException("Los puntos karma deben ser positivos.");

            PlantillaId = id_PlantillaTarea;
            Nombre = nombre;
            FechaCreacion = fechaCreacion;
            PuntosKarma = puntosKarma;
            Estado = estado;
        }
    }
}
