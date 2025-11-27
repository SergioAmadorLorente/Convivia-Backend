using System.Text.Json.Serialization;

namespace Convivia.Domain.Entities
{
    /// <summary>
    /// Entidad de dominio: Peticion
    /// Representa una solicitud de un usuario para unirse a un espacio
    /// </summary>
    public class Peticion
    {
        public string Id { get; set; }
        public string Mensaje { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; }
        public string IdSolicitante { get; set; }
        public string IdEspacio { get; set; }

        // Constructor privado para reconstruir desde persistencia
        private Peticion() { }

        // Constructor para crear nueva peticion
        public Peticion(string id, string mensaje, string idSolicitante, string idEspacio)
        {
            if (string.IsNullOrWhiteSpace(mensaje))
                throw new ArgumentException("El mensaje no puede estar vacío.", nameof(mensaje));
            if (string.IsNullOrWhiteSpace(idSolicitante))
                throw new ArgumentException("El ID del solicitante no puede estar vacío.", nameof(idSolicitante));
            if (string.IsNullOrWhiteSpace(idEspacio))
                throw new ArgumentException("El ID del espacio no puede estar vacío.", nameof(idEspacio));

            Id = id;
            Mensaje = mensaje;
            IdSolicitante = idSolicitante;
            IdEspacio = idEspacio;
            Fecha = DateTime.UtcNow;
            Estado = "pendiente";
        }

        // Factory method para reconstruir desde persistencia
        public static Peticion Reconstruir(string id, string mensaje, DateTime fecha, string estado, string idSolicitante, string idEspacio)
        {
            return new Peticion
            {
                Id = id,
                Mensaje = mensaje,
                Fecha = fecha,
                Estado = estado,
                IdSolicitante = idSolicitante,
                IdEspacio = idEspacio
            };
        }

        // Métodos de negocio
        public void Aceptar()
        {
            if (Estado != "pendiente")
                throw new InvalidOperationException($"No se puede aceptar una petición en estado '{Estado}'.");
            Estado = "aceptada";
        }

        public void Rechazar()
        {
            if (Estado != "pendiente")
                throw new InvalidOperationException($"No se puede rechazar una petición en estado '{Estado}'.");
            Estado = "rechazada";
        }

        public void Cancelar()
        {
            if (Estado != "pendiente")
                throw new InvalidOperationException($"No se puede cancelar una petición en estado '{Estado}'.");
            Estado = "cancelada";
        }

        public bool ValidarEstado()
        {
            var estadosValidos = new List<string> { "pendiente", "aceptada", "rechazada", "cancelada" };
            return estadosValidos.Contains(Estado);
        }

        public bool EsPendiente() => Estado == "pendiente";
        public bool EstaAceptada() => Estado == "aceptada";
        public bool EstaRechazada() => Estado == "rechazada";
        public bool EstaCancelada() => Estado == "cancelada";
    }
}