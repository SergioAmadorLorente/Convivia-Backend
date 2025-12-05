using Convivia.Domain.Entities;
using Google.Cloud.Firestore;
using System.Text.Json.Serialization;
namespace Convivia.Domain.Entities
{


    public class UsuarioEspacio
    {
        public string Id_UsuarioEspacio { get; set; } = Guid.NewGuid().ToString();

        public bool Ausente { get; set; }

        public int Karma { get; set; }

        public string Rol { get; set; }

        public Espacio Espacio { get; set; }

        public Usuario Usuario { get; set; }

        public List<Tarea> tareas { get; set; } = new();

        public Permiso Permiso { get; set; }

        public List<Factura> Facturas { get; set; } = new();


        // ?

        // Constructor por defecto vacio para pruebas
        public UsuarioEspacio()
        {
        }

        // Constructor que asigna el permiso según el rol proporcionado
        public UsuarioEspacio(Usuario usuario, Espacio espacio, Permiso permiso, string rol, bool ausente = false, int karma = 0)
        {
            Id_UsuarioEspacio = Guid.NewGuid().ToString();
            this.Usuario = usuario;
            this.Espacio = espacio;
            this.Ausente = ausente;
            this.Karma = karma;

            if (rol == "admin")
            {
                var rolAdmin = new Rol();
                rolAdmin.SetConfigurarcionAdmin();
                this.Permiso = new Permiso(
                    rolAdmin,
                    crearTarea: rolAdmin.CrearTarea,
                    eliminarTarea: rolAdmin.EliminarTarea,
                    editarTarea: rolAdmin.EditarTarea,
                    añadirUsuario: rolAdmin.AñadirUsuario,
                    eliminarUsuario: rolAdmin.EliminarUsuario,
                    asignarTarea: rolAdmin.AsignarTarea,
                    asignarseTarea: rolAdmin.AsignarseTarea
                );
            }
            else
            {
                // Por defecto asignar rol Usuario
                var rolUsuario = new Rol();
                rolUsuario.SetConfigurarcionUsuario();
                this.Permiso = new Permiso(
                    rolUsuario,
                    crearTarea: rolUsuario.CrearTarea,
                    eliminarTarea: rolUsuario.EliminarTarea,
                    editarTarea: rolUsuario.EditarTarea,
                    añadirUsuario: rolUsuario.AñadirUsuario,
                    eliminarUsuario: rolUsuario.EliminarUsuario,
                    asignarTarea: rolUsuario.AsignarTarea,
                    asignarseTarea: rolUsuario.AsignarseTarea
                );
            }
        }

        // Constructor que recibe un objeto Permiso directamente
        public UsuarioEspacio(Usuario usuario, Espacio espacio, Permiso permiso, bool ausente = false, int karma = 0)
        {
            Id_UsuarioEspacio = Guid.NewGuid().ToString();
            this.Usuario = usuario;
            this.Espacio = espacio;
            this.Ausente = ausente;
            this.Karma = karma;
            this.Permiso = permiso;
        }
    }
}