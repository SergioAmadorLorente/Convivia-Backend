using Convivia.Domain.Entities;

namespace Convivia.Domain.Entities
{
    public class Invitacion
    {
        public string Id { get; internal set; } = Guid.NewGuid().ToString(); // El setter només sera accesible de manera interna, per a poder utilitzarlo en el repo de firestore

        public UsuarioEspacio? UsuarioSolicitante { get; set; }

        public Usuario? UsuarioInvitado { get; set; }

        public Espacio? Espacio { get; set; }

        public string? Mensaje { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        public string Estado { get; set; } = "pendiente";

        public Invitacion() { }

        public bool ValidarEstado()
        {
            var estadosValidos = new List<string> { "pendiente", "aceptada", "rechazada", "cancelada" };
            return estadosValidos.Contains(Estado);
        }


        public void Aceptar()
        {
            Estado = "aceptada";
        }

        public void Rechazar()
        {
            Estado = "rechazada";
        }

        public void Pendiente()
        {
            Estado = "pendiente";
        }

        public void Cancelar()
        {
            Estado = "cancelada";
        }
    }
}