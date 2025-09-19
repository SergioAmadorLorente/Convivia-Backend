using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AuthApiDemo.Models
{
    public class PlantillaTarea
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id_PlantillaTarea { get; set; }

        [BsonElement("nombre")]
        public string Nombre { get; set; }

        [BsonElement("repeticion")]
        public int Repeticion { get; set; }

        [BsonElement("fechaCreacion")]
        public DateTime FechaCreacion { get; set; }

        [BsonElement("puntosKarma")]
        public int PuntosKarma { get; set; }

        [BsonElement("disponible")]
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
