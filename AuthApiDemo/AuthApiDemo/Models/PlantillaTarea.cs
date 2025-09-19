

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
            Id_PlantillaTarea = id_PlantillaTarea;
            Nombre = nombre;
            Repeticion = repeticion;
            FechaCreacion = fechaCreacion;
            PuntosKarma = puntosKarma;
            Disponible = disponible;
        }

    }
}
