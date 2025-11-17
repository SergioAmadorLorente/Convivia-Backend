using Google.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convivia.Application.DTOs
{
    internal class CreateInvitacionDto
    {
        public string UsuarioSolcitanteId { get; set; }
        public string UsuarioInvitadoId { get; set; }
        public string EspacioId { get; set; }
        public string? Mensaje { get; set; }
    }
}
