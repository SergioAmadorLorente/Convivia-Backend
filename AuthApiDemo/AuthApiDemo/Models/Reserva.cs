using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AuthApiDemo.Models
{
    public class Reserva
    {

        public string Id_Reserva { get; set; } = Guid.NewGuid().ToString();

        public DateTime FechaInicial { get; set; }

        public DateTime FechaFinal { get; set; }

        public string Id_Sala { get; set; }
        public Usuario usuario { get; set; }

        public Reserva()
        {
            // Constructor vacío necesario para la deserialización
        }

        public Reserva(DateTime fechaInicial, DateTime fechaFinal, string id_Sala)
        {
            FechaInicial = fechaInicial;
            FechaFinal = fechaFinal;
            Id_Sala = id_Sala;
        }

    }
}
