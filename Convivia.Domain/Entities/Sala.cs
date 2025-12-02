using System;

namespace Convivia.Domain.Entities
{
    public class Sala
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public string IdEspacio { get; set; } = string.Empty;

        public Sala() { }

        public Sala(string nombre, string idEspacio, string? descripcion = null)
        {
            Nombre = nombre ?? string.Empty;
            IdEspacio = idEspacio ?? string.Empty;
            Descripcion = descripcion;
        }
    }
}