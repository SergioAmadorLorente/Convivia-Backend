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
    

    public bool validarEstado()
        {
            var estadosValidos = new List<string> { "pendiente", "aceptada", "rechazada", "cancelada" };
            return estadosValidos.Contains(Estado);
        }


        public void aceptar()
        {
            this.Estado = "aceptada";
        }

        public void rechazar()
        {
            this.Estado = "rechazada";
        }

        public void pendiente()
        {
            this.Estado = "pendiente";
        }

        public void cancelar()
        {
            this.Estado = "cancelada";
        }

    }
}