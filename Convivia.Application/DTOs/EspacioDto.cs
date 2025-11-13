using System;
using System.Collections.Generic;

namespace Convivia.Application.DTOs
{
    public class EspacioDto
    {
        public string Id_Espacio { get; set; }
        public string Nombre { get; set; }
        public string? Direccion { get; set; }
        public List<string>? SalaIds { get; set; }
        public List<string>? UsuarioEspacioIds { get; set; }
        public List<string>? PeticionIds { get; set; }
        public List<string>? InvitacionIds { get; set; }
    }
}