using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Firestore;


namespace Convivia.Shared.DTOs
{
    public class EspacioDto
    {
        public EspacioDto() { }
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string? Direccion { get; set; }
    }
}
