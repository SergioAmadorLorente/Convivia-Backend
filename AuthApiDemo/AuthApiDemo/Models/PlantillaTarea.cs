namespace AuthApiDemo.Models
{
    public class PlantillaTarea
    {
        public string Id_PlantillaTarea { get; set; }

        public string Nombre { get; set; }

        public int Repeticion { get; set; }

        public DateTime FechaCreacion { get; set; }

        public int PuntosKarma { get; set; }

        public bool Disponible { get; set; }

        public PlantillaTarea()
        {
        }

        public PlantillaTarea(string id_PlantillaTarea, string nombre, int repeticion, DateTime fechaCreacion, int puntosKarma, bool disponible)
        {
            if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre no puede estar vacío.");
            if (repeticion < 0) throw new ArgumentException("La repetición debe ser positiva.");
            if (puntosKarma < 0) throw new ArgumentException("Los puntos karma deben ser positivos.");

            Id_PlantillaTarea = id_PlantillaTarea;
            Nombre = nombre;
            Repeticion = repeticion;
            FechaCreacion = fechaCreacion;
            PuntosKarma = puntosKarma;
            Disponible = disponible;
        }
        // TODO Cuando hagan plantilla tarea podreis configurar el metodo UsuarioEspacio.crearTareaDePlantilla(PlantillaTarea plantilla) no tengo muy claro que hace ni para que existe plantilla tarea, entonces mee cuesta imaginar el metodo ;-)
        // Firmado Marc Sastre


        public void editarPlantilla(string nombre, int repeticion, int puntosKarma)
        {
            if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre no puede estar vacío.");
            if (repeticion < 0) throw new ArgumentException("La repetición debe ser positiva.");
            if (puntosKarma < 0) throw new ArgumentException("Los puntos karma deben ser positivos.");

            Nombre = nombre;
            Repeticion = repeticion;
            PuntosKarma = puntosKarma;
        }

        public void cambiarDisponibilidad()
        {
            this.Disponible = !this.Disponible;
        }

        public void programarRecurrencia(int repeticion)
        {
            this.Repeticion = repeticion;
        }

        public bool Validar()
        {
            return !string.IsNullOrWhiteSpace(Nombre) && Repeticion >= 0 && PuntosKarma >= 0;
        }
    }
}
