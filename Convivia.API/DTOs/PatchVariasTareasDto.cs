using System.Collections.Generic;

namespace AuthApiDemo.DTOs
{
    public class PatchVariasTareasDto
    {
        public List<string> ListaIds { get; set; } = new List<string>();
        public UpdateTareaDto Dto { get; set; } = new UpdateTareaDto();
    }
}
