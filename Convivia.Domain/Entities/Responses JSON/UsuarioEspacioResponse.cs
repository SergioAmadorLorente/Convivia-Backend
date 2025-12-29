public class UsuarioEspacioResponse
{
    public string Id_UsuarioEspacio { get; set; }
    public bool Ausente { get; set; }
    public int karma { get; set; }
    public string Rol { get; set; }

    public string UsuarioId { get; set; }
    public string EspacioId { get; set; }
    public string PermisoId { get; set; }

    public List<string> tareas { get; set; } = new();
    public List<string> Facturas { get; set; } = new();
}