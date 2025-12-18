
using System;
using Convivia.Domain.Entities;

namespace Convivia.Domain.Entities
{
    public class Reserva
    {

        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string? description { get; set; }

        public DateTime startTime { get; set; } = DateTime.Now;

        public DateTime? endTime { get; set; }

        public string idSala { get; set; }
        public string idUser { get; set; }

        public Reserva()
        {
            // Constructor vacío necesario para la deserialización
        }

        public Reserva(string? description, DateTime startTime, DateTime? endTime, string idSala, string idUser)
        {
            description = description;
            startTime = startTime;
            endTime = endTime;
            this.idSala = idSala;
            this.idUser = idUser;
        }

    }
}
