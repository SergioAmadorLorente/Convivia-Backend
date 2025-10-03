using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AuthApiDemo.DTOs
{
    public class CreateEspacioDto
    {
        [Required]
        public string Nombre { get; set; }
        public string? Direccion { get; set; }
        public List<string>? SalaIds { get; set; }
        public List<string>? UsuarioEspacioIds { get; set; }
        public List<string>? PeticionIds { get; set; }
        public List<string>? InvitacionIds { get; set; }
    }
}