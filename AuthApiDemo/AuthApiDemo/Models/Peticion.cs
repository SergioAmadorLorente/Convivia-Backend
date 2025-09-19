namespace AuthApiDemo.Models
{
    public class Peticion
    {

        public string Id { get; set; } = Guid.NewGuid().ToString();

        public Usuario Solicitante { get; set; }

        public string Mensaje { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        public string Estado { get; set; } = "pendiente";
    }
}