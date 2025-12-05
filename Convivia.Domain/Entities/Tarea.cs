using Convivia.Domain.Entities;
using Google.Cloud.Firestore;
using System.Text.Json.Serialization;

namespace Convivia.Domain.Entities
{

    public class Tarea
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        public string Nombre { get; set; }

        public List<UsuarioEspacio> Usuarios { get; set; } = new();
        
        public DateTime? FechaRealizacion { get; set; }

        public DateTime HoraLimite { get; set; }

        public byte[]? Foto { get; set; } // Para almacenar imagen binaria

        public DateTime? Prorroga { get; set; } // Puede ser null

        public bool Estado { get; set; }

        public Espacio espacio { get; set; }

        public string? FacturaId { get; set; }
        public string EspacioId { get; set; }
        public string PlantillaId { get; set; } = string.Empty; // obligatorio
        public List<string> UsuarioEspaciosIds { get; set; } = new();

        public Factura? Factura { get; set; }

        public List<int> DiasRepeticion { get; set; } = new();

        public int karma { get; set; }

        // Optional sala id where task applies
        public string? SalaId { get; set; }

        public Tarea()
        {
            // Constructor vacío necesario para la deserialización
        }

        public Tarea(string nombre, List<string> usuarioEspaciosIds, DateTime horaLimite, int karma, string espacioId, string plantillaId, string? facturaId = null)
        {
            if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre no puede estar vacío.");
            if (karma < 0) throw new ArgumentException("Los puntos karma deben ser positivos.");
            if (string.IsNullOrWhiteSpace(plantillaId)) throw new ArgumentException("PlantillaId es obligatoria.");

            Nombre = nombre;
            UsuarioEspaciosIds = usuarioEspaciosIds;
            HoraLimite = horaLimite;
            this.karma = karma;
            Estado = false; // Por defecto, la tarea está incompleta
            EspacioId = espacioId;
            PlantillaId = plantillaId;
            FacturaId = facturaId;
        }
    }
}