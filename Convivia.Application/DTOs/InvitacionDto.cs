using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convivia.Application.DTOs
{
    internal class InvitacionDto
    {
        public string Id { get; set; }
        public string UsuarioSolicitanteId { get; set; }
        public string UsuarioInvitadoId { get; set; }
        public string EspacioId { get; set; }
        public string Mensaje { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; }

    }
}
