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
                this.Permiso = Permiso.Admin;
            }
            else
            {
                // Por defecto asignar rol Usuario
                this.Permiso = Permiso.Usuario;
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