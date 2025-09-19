namespace AuthApiDemo.Models
{

    public class UsuarioEspacio
    {

        public string Id_UsuarioEspacio { get; set; } = Guid.NewGuid().ToString();

        public bool Ausente { get; set; }

        public int Karma { get; set; }

        public bool Admin { get; set; }

        public Espacio espacio { get; set; }

        public Usuario usuario { get; set; }

        public UsuarioEspacio()
        {
        }

        public UsuarioEspacio(Usuario usuario, Espacio espacio, bool ausente = false, int karma = 0)
        {
            Id_UsuarioEspacio = Guid.NewGuid().ToString();
            this.usuario = usuario;
            this.espacio = espacio;
            Ausente = ausente;
            Karma = karma;
        }


        public void invitarUsuario(Usuario usuario)
        {

            Invitacion invitacion = new Invitacion
            {
                Id = Guid.NewGuid().ToString(),
                Espacio = this.espacio,
                UsuarioSolicitante = this,
                Fecha = DateTime.UtcNow,
                Estado = "pendiente"
            };

            usuario.Invitaciones.Add(invitacion);
            this.espacio.InvitacionesEnviadas.Add(invitacion);

        }

    }
}

