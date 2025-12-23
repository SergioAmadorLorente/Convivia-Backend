using Convivia.Domain.Entities;
using Google.Cloud.Firestore;
using System.Text.Json.Serialization;
namespace Convivia.Domain.Entities
{


    public class UsuarioEspacio
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        public bool Ausente { get; set; }

        public int Karma { get; set; }

        public string Rol { get; set; }

        public string EspacioId { get; set; } = string.Empty;

        public string UsuarioId { get; set; } = string.Empty;

        public List<string> TareasId { get; set; } = new();

        public string PermisoId { get; set; } = string.Empty;

        public List<string> FacturasId { get; set; } = new();

        // Propiedades de navegación (para cuando se necesiten los objetos completos)
        public Espacio Espacio { get; set; }

        public Usuario Usuario { get; set; }

        public List<Tarea> tareas { get; set; } = new();

        public Permiso Permiso { get; set; }

        public List<Factura> Facturas { get; set; } = new();


        // Constructor por defecto vacio para pruebas
        public UsuarioEspacio()
        {
        }

        // Constructor que asigna el permiso según el rol proporcionado
        public UsuarioEspacio(Usuario usuario, Espacio espacio, Permiso permiso, string rol, bool ausente = false, int karma = 0)
        {
            Id = Guid.NewGuid().ToString("N");
            this.Usuario = usuario;
            this.UsuarioId = usuario?.Id ?? string.Empty;
            this.Espacio = espacio;
            this.EspacioId = espacio?.Id ?? string.Empty;
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
            this.PermisoId = this.Permiso?.Id ?? string.Empty;
        }

        // Constructor que recibe un objeto Permiso directamente
        public UsuarioEspacio(Usuario usuario, Espacio espacio, Permiso permiso, bool ausente = false, int karma = 0)
        {
            Id = Guid.NewGuid().ToString("N");
            this.Usuario = usuario;
            this.UsuarioId = usuario?.Id ?? string.Empty;
            this.Espacio = espacio;
            this.EspacioId = espacio?.Id ?? string.Empty;
            this.Ausente = ausente;
            this.Karma = karma;
            this.Permiso = permiso;
            this.PermisoId = permiso?.Id ?? string.Empty;
        }
    }
}