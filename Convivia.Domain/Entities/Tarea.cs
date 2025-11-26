/*
using Google.Cloud.Firestore;
using System.Text.Json.Serialization;

namespace Convivia.Domain.Models
{
    [FirestoreData]
    public class Tarea
    {
        [FirestoreProperty]
        public string Id_Tarea { get; set; } = Guid.NewGuid().ToString();

        [JsonIgnore]
        public List<UsuarioEspacio> Usuarios { get; set; }

        [FirestoreProperty]
        public DateTime FechaRealizacion { get; set; }

        [FirestoreProperty]
        public DateTime FechaLimite { get; set; }

        [FirestoreProperty]
        public byte[]? Foto { get; set; } // Para almacenar imagen binaria

        [FirestoreProperty]
        public DateTime? Prorroga { get; set; } // Puede ser null

        [FirestoreProperty]
        public bool Estado { get; set; }

        [JsonIgnore]
        public Espacio espacio { get; set; }

        [JsonIgnore]
        public Factura? Factura { get; set; }
        [FirestoreProperty]
        public int karma { get; set; } = 10; // Puntos de karma que se otorgan al completar la tarea

        public Tarea()
        {
            // Constructor vacío necesario para la deserialización
        }

        public Tarea(List<UsuarioEspacio> usuarios, DateTime fechaRealizacion, DateTime fechaLimite, int karma, byte[]? foto = null, DateTime? prorroga = null, bool estado = false)
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
                listausuarios[i].tareas.Add(this);
            }

        }

        public void quitarUsuario(UsuarioEspacio usuario)
        {
            Usuarios.Remove(usuario);
            usuario.tareas.Remove(this);
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
                usuario.tareas.Remove(this);
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
            Factura = new Factura(Id_Tarea,
                "Factura de Tarea " + Id_Tarea,
                precio,
                usuariosapagar,
                false,
                null,
                this);

        }

    }
}
*/