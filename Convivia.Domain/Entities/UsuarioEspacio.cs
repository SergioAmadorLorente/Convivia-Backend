using Convivia.Domain.Entities;
using Google.Cloud.Firestore;
using System.Text.Json.Serialization;
namespace Convivia.Domain.Entities
{


    [FirestoreData]
    public class UsuarioEspacio
    {
        [FirestoreProperty]
        public string Id_UsuarioEspacio { get; set; } = Guid.NewGuid().ToString();

        [FirestoreProperty]
        public bool Ausente { get; set; }

        [FirestoreProperty]
        public int Karma { get; set; }

        [FirestoreProperty]
        public string Rol { get; set; }

        [FirestoreProperty]
        public DocumentReference EspacioRef { get; set; }

        [JsonIgnore]
        public Espacio Espacio { get; set; }


        [FirestoreProperty]
        public DocumentReference UsuarioRef { get; set; }


        [JsonIgnore]
        public Usuario Usuario { get; set; }


        [FirestoreProperty("tareas")]
        public List<DocumentReference> tareasRefs { get; set; } = new();

        [JsonIgnore]
        public List<Tarea> tareas { get; set; } = new();


        [FirestoreProperty]
        public DocumentReference PermisoRef { get; set; }


        [JsonIgnore]
        public Permiso Permiso { get; set; }


        [FirestoreProperty]
        public List<DocumentReference> FacturasRefs { get; set; } = new();

        [JsonIgnore]
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
            else if (rol == "usuario")
            {
                this.Permiso = Permiso.Usuario;
            }
            else
            {
                this.Permiso = Permiso.Huesped;
            }
        }

        // JSON

        public UsuarioEspacioResponse ToResponse()
        {
            return new UsuarioEspacioResponse
            {
                Id_UsuarioEspacio = this.Id_UsuarioEspacio,
                Ausente = this.Ausente,
                Karma = this.Karma,
                Rol = this.Rol,
                UsuarioId = UsuarioRef?.Id,
                EspacioId = EspacioRef?.Id,
                PermisoId = PermisoRef?.Id,
                tareas = tareasRefs?.Select(r => r.Id).ToList() ?? new(),
                Facturas = FacturasRefs?.Select(r => r.Id).ToList() ?? new()
            };
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