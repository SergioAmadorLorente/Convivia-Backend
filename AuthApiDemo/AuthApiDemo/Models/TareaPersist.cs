using System;
using System.Collections.Generic;

namespace AuthApiDemo.Models
{
    public class TareaPersist
    {
        public string IdTarea { get; set; } = default!;
        public string Nombre { get; set; } = default!;
        public DateTime FechaLimite { get; set; }
        public string? Descripcion { get; set; }
        public List<string> UsuarioEspacioIds { get; set; } = new();
        public int Karma { get; set; } = 10;
        public byte[]? Foto { get; set; }
        public DateTime? Prorroga { get; set; }
        public bool Estado { get; set; } = false;
        public DateTime? FechaRealizacion { get; set; }
    }
}