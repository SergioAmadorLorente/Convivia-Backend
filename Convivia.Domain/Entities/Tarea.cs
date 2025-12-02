using Convivia.Domain.Entities;
using Google.Cloud.Firestore;
using System.Text.Json.Serialization;

namespace Convivia.Domain.Entities
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

        

    }
}