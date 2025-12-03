using Convivia.Domain.Entities;
using Google.Cloud.Firestore;
using System.Text.Json.Serialization;

namespace Convivia.Domain.Entities
{

    public class Tarea
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        public string Nombre { get; set; }

        public List<UsuarioEspacio> Usuarios { get; set; }
        
        public DateTime? FechaRealizacion { get; set; }

        public DateTime HoraLimite { get; set; }

        public byte[]? Foto { get; set; } // Para almacenar imagen binaria

        public DateTime? Prorroga { get; set; } // Puede ser null

        public bool Estado { get; set; }

        public Espacio espacio { get; set; }

        public string? FacturaId { get; set; }
        public string EspacioId { get; set; }
        public string? PlantillaId { get; set; }
        public List<string> UsuarioEspaciosIds { get; set; } = new();

        public Factura? Factura { get; set; }

        public int PuntosKarma { get; set; } = 10; // Puntos de karma que se otorgan al completar la tarea

        public Tarea()
        {
            // Constructor vacío necesario para la deserialización
        }

        public Tarea(string nombre, List<string> usuarioEspaciosIds, DateTime horaLimite, int puntosKarma, string espacioId, string? plantillaId = null, string? facturaId = null)
        {
            if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre no puede estar vacío.");
            if (puntosKarma < 0) throw new ArgumentException("Los puntos karma deben ser positivos.");
            Nombre = nombre;
            UsuarioEspaciosIds = usuarioEspaciosIds;
            HoraLimite = horaLimite;
            PuntosKarma = puntosKarma;
            Estado = false; // Por defecto, la tarea está incompleta
            EspacioId = espacioId;
            PlantillaId = plantillaId;
            FacturaId = facturaId;
        }
    }
}