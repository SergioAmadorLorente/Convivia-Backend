namespace AuthApiDemo.Models
{
    public class Tarea
    {

        public string Id_Tarea { get; set; } = Guid.NewGuid().ToString();

        public UsuarioEspacio[] Usuarios { get; set; }

        public DateTime FechaRealizacion { get; set; }

        public DateTime FechaLimite { get; set; }

        public byte[]? Foto { get; set; } // Para almacenar imagen binaria

        public DateTime? Prorroga { get; set; } // Puede ser null

        public bool Estado { get; set; }

        public Tarea()
        {
            // Constructor vacío necesario para la deserialización
        }

        public Tarea(UsuarioEspacio[] usuarios, DateTime fechaRealizacion, DateTime fechaLimite, byte[]? foto = null, DateTime? prorroga = null, bool estado = false)
        {
            Usuarios = usuarios;
            FechaRealizacion = fechaRealizacion;
            FechaLimite = fechaLimite;
            Foto = foto;
            Prorroga = prorroga;
            Estado = estado;
        }

        public void agregarUsuarios(UsuarioEspacio usuarioespacio)
        {

            // Usuarios.add = usuarioespacio

        }

    }
}
