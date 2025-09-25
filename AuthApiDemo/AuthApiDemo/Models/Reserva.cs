namespace AuthApiDemo.Models
{
    public class Reserva
    {

        public string Id_Reserva { get; set; } = Guid.NewGuid().ToString();

        public DateTime FechaInicial { get; set; }

        public DateTime FechaFinal { get; set; }

        public Sala sala { get; set; }
        public Usuario usuario { get; set; }

        public Reserva()
        {
            // Constructor vacío necesario para la deserialización
        }

        public Reserva(DateTime fechaInicial, DateTime fechaFinal, Sala sala)
        {
            FechaInicial = fechaInicial;
            FechaFinal = fechaFinal;
            sala = sala;
        }

        public bool reprogramarFecha(DateTime fechaInicial, DateTime fechaFinal)
        {
            if (sala.esDisponible(fechaInicial, fechaFinal))
            {
                FechaInicial = fechaInicial;
                FechaFinal = fechaFinal;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}