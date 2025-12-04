using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convivia.Shared.DTOs
{
    public class PermisoDto
    {
        public string Id { get; set; }
        public string Rol { get; set; }
        public bool CrearTarea { get; set; }
        public bool EliminarTarea { get; set; }
        public bool EditarTarea { get; set; }
        public bool AñadirUsuario { get; set; }
        public bool EliminarUsuario { get; set; }
        public bool AsignarTarea { get; set; }
        public bool AsignarseTarea { get; set; }

        public PermisoDto() { }
    }
}
