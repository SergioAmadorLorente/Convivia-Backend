using Google.Cloud.Firestore;
using System.Text.Json.Serialization;

namespace Convivia.Domain.Models
{
    [FirestoreData]
    public class Peticion
    {
        [FirestoreProperty]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonIgnore]
        public Usuario Solicitante { get; set; }

        [FirestoreProperty]
        public string Mensaje { get; set; }

        [FirestoreProperty]
        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        [FirestoreProperty]
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