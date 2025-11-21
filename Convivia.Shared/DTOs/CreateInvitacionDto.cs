using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convivia.Shared.DTOs
{
    public class CreateInvitacionDto
    {
        public string UsuarioSolicitanteId { get; set; }
        public string UsuarioInvitadoId { get; set; }
        public string EspacioId { get; set; }
        public string? Mensaje { get; set; }
    }
}
