namespace Convivia.Domain.Entities
{
    public class PlantillaTarea
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public int karma { get; set; }

        public bool Estado { get; set; }

        public List<int> DiasRepeticion { get; set; } = new List<int>();

        // public List<Tarea> tareas { get; set; } = new List<Tarea>();

        public PlantillaTarea()
        {
        }

        public PlantillaTarea(string id_PlantillaTarea, string nombre, DateTime fechaCreacion, int karma, bool estado, int repeticion)
        {
            if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre no puede estar vacío.");
            if (karma < 0) throw new ArgumentException("Los puntos karma deben ser positivos.");

            Id = id_PlantillaTarea;
            Nombre = nombre;
            FechaCreacion = fechaCreacion;
            this.karma = karma;
            Estado = estado;
        }
    }
}
