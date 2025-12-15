using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Convivia.Shared.DTOs
{
    public class RolDto
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string Nombre { get; set; }
        public bool CrearTarea { get; set; }
        public bool EliminarTarea { get; set; }
        public bool EditarTarea { get; set; }
        public bool AñadirUsuario { get; set; }
        public bool EliminarUsuario { get; set; }
        public bool AsignarTarea { get; set; }
        public bool AsignarseTarea { get; set; }

        public RolDto() { }
    }
}
