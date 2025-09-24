namespace AuthApiDemo.Models
{
    public class Invitacion
    {

        public string Id { get; set; } = Guid.NewGuid().ToString();

        public UsuarioEspacio UsuarioSolicitante { get; set; }
        
        public Usuario UsuarioInvitado { get; set; }

        public Espacio Espacio { get; set; }

        public string Mensaje { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        public string Estado { get; set; } = "pendiente";
    }
}