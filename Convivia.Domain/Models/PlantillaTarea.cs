namespace Convivia.Domain.Models
{
    public class PlantillaTarea
    {
        public string PlantillaId { get; set; }

        public string Nombre { get; set; }

        public DateTime FechaCreacion { get; set; }

        public int PuntosKarma { get; set; }

        public bool Disponible { get; set; }

        public List<int> DiasRepeticion { get; set; } = new List<int>();

        public List<Tarea> tareas { get; set; } = new List<Tarea>();

        public List<string> TareasId { get; set; } = new List<string>();

        public PlantillaTarea()
        {
        }

        public PlantillaTarea(string id_PlantillaTarea, string nombre, DateTime fechaCreacion, int puntosKarma, bool disponible, int repeticion)
        {
            if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre no puede estar vacío.");
            if (puntosKarma < 0) throw new ArgumentException("Los puntos karma deben ser positivos.");

            PlantillaId = id_PlantillaTarea;
            Nombre = nombre;
            FechaCreacion = fechaCreacion;
            PuntosKarma = puntosKarma;
            Disponible = disponible;
        }
        public void editarPlantilla(string nombre, int repeticion, int puntosKarma)
        {
            if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre no puede estar vacío.");
            if (repeticion < 0) throw new ArgumentException("La repetición debe ser positiva.");
            if (puntosKarma < 0) throw new ArgumentException("Los puntos karma deben ser positivos.");

            Nombre = nombre;
            PuntosKarma = puntosKarma;
        }

        public void cambiarDisponibilidad()
        {
            this.Disponible = !this.Disponible;
        }
    }
}
