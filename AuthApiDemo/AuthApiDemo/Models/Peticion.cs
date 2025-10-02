using System.Text.Json.Serialization;

namespace AuthApiDemo.Models
{
    public class Peticion
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonIgnore]
        public Usuario Solicitante { get; set; }

        public string Mensaje { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        public string Estado { get; set; } = "pendiente";

        public bool ValidarEstado()
        {
            var estadosValidos = new List<string> { "pendiente", "aceptada", "rechazada", "cancelada" };
            return estadosValidos.Contains(Estado);
        }

        public void Aceptar() => Estado = "aceptada";

        public void Rechazar() => Estado = "rechazada";

        public void Pendiente() => Estado = "pendiente";

        public void Cancelar() => Estado = "cancelada";
    }
}