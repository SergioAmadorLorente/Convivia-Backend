namespace AuthApiDemo.Models
{
    public class Tarea
    {

        public string Id_Tarea { get; set; } = Guid.NewGuid().ToString();

        public List<UsuarioEspacio> Usuarios { get; set; }

        public DateTime FechaRealizacion { get; set; }

        public DateTime FechaLimite { get; set; }

        public byte[]? Foto { get; set; } // Para almacenar imagen binaria

        public DateTime? Prorroga { get; set; } // Puede ser null

        public bool Estado { get; set; }

        public Factura? Factura { get; set; }
        public int karma { get; set; } = 10; // Puntos de karma que se otorgan al completar la tarea

        public Tarea()
        {
            // Constructor vacío necesario para la deserialización
        }

        public Tarea(List<UsuarioEspacio> usuarios, DateTime fechaRealizacion, DateTime fechaLimite, byte[]? foto = null, DateTime? prorroga = null, bool estado = false, int karma)
        {
            Usuarios = usuarios;
            FechaRealizacion = fechaRealizacion;
            FechaLimite = fechaLimite;
            Foto = foto;
            Prorroga = prorroga;
            Estado = estado;
            this.karma = karma;
        }

        public void agregarUsuarios(List<UsuarioEspacio> listausuarios)
        {

            Usuarios.AddRange(listausuarios);
            for (int i = 0; i < listausuarios.Count; i++)
            {
                listausuarios[i].TareasAsignadas.Add(this);
            }

        }

        public void quitarUsuario(UsuarioEspacio usuario)
        {
            Usuarios.Remove(usuario);
            usuario.TareasAsignadas.Remove(this);
        }

        public void editarTarea(DateTime nuevaFechaLimite, byte[]? nuevaFoto = null, DateTime? nuevaProrroga = null, bool nuevoEstado = false)
        {
            FechaLimite = nuevaFechaLimite;
            if (nuevaFoto != null)
            {
                Foto = nuevaFoto;
            }
            Prorroga = nuevaProrroga;
            Estado = nuevoEstado;
        }

        public void eliminarTarea()
        {
            foreach (var usuario in Usuarios)
            {
                usuario.TareasAsignadas.Remove(this);
            }
            Usuarios.Clear();
        }

        public void cambiarEstado(bool nuevoEstado)
        {
            Estado = nuevoEstado;
        }

        public void adjuntarFoto(byte[] foto)
        {
            Foto = foto;
        }

        public void haExpirado()
        {
            if (DateTime.UtcNow > FechaLimite)
            {
                Estado = false;
            }
        }

        public void prorrogarTarea(DateTime fechanueva)
        {

            Prorroga = fechanueva;

        }

        public void crearFactura(List<UsuarioEspacio> usuariosapagar, int precio)
        {

            Factura = new Factura
            {
                Nombre = "Factura de Tarea " + Id_Tarea,
                Precio = precio,
                Reparto = usuariosapagar,
                Pagado = false,
                Documento = null,
                tarea = this
            };

        }

    }
}