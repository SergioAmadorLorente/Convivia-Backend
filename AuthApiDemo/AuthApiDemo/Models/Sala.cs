using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AuthApiDemo.Models
{
    public class Sala
    {

        public string Id_Sala { get; set; } = Guid.NewGuid().ToString();

        public string Nombre { get; set; }

        public string? Descripcion { get; set; } // Puede ser null

        public string Id_Espacio { get; set; } // Solo el ID

        private List<Reserva> reservas = new List<Reserva>();
        [JsonIgnore]
        public Espacio? Espacio { get; set; } // Opcional: objeto completo

        public Sala()
        {
            
        }

        public Sala(string nombre, string id_Espacio, string? descripcion = null)
        {
            Nombre = nombre;
            Descripcion = descripcion;
            Id_Espacio = id_Espacio;
        }
        public void listarReservas()
        {
            foreach (var reserva in reservas)
            {
                Console.WriteLine($"Reserva ID: {reserva.Id_Reserva}, Fecha Inicial: {reserva.FechaInicial}, Fecha Final: {reserva.FechaFinal}, Usuario: {reserva.usuario.Nombre}");
            }
        }

        public bool esDisponible(DateTime fechaInicio, DateTime fechaFin)
        {
            foreach (var reserva in reservas)
            {
                if (fechaInicio < reserva.FechaFinal && fechaFin > reserva.FechaInicial)
                {
                    Console.WriteLine("No disponible");
                    return false;
                }
            }
            return true;
        }
        public bool crearReserva(DateTime fechaInicio, DateTime fechaFin, Usuario usuario)
        {
            if (esDisponible(fechaInicio, fechaFin))
            {
                Reserva nuevaReserva = new Reserva
                {
                    Id_Reserva = Guid.NewGuid().ToString(),
                    FechaInicial = fechaInicio,
                    FechaFinal = fechaFin,
                    Id_Sala = this.Id_Sala,
                    usuario = usuario
                };
                reservas.Add(nuevaReserva);
                return true;
            }
            else
            {
                Console.WriteLine("No se pudo crear la reserva, el espacio no está disponible en las fechas solicitadas.");
                return false;
            }
        }

        public bool eliminarReserva(string idReserva)
        {
            var reserva = reservas.FirstOrDefault(r => r.Id_Reserva == idReserva);
            if (reserva != null)
            {
                reservas.Remove(reserva);
                return true;
            }
            else
            {
                Console.WriteLine("Reserva no encontrada.");
                return false;
            }
        }
        public bool eliminarReserva(Reserva reserva)
        {
            if (reservas.Contains(reserva))
            {
                reservas.Remove(reserva);
                return true;
            }
            else
            {
                Console.WriteLine("Reserva no encontrada.");
                return false;
            }
        }
        public Reserva buscarReserva(string idReserva)
        {
            return reservas.FirstOrDefault(r => r.Id_Reserva == idReserva);
        }

    }
}
