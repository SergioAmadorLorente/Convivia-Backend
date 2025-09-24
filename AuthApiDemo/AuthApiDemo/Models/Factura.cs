namespace AuthApiDemo.Models
{
    public class Factura
    {

        public string Id_Factura { get; set; } = Guid.NewGuid().ToString();

        public string Nombre { get; set; }

        public int Precio { get; set; }

        public UsuarioEspacio[] Reparto { get; set; }

        public bool Pagado { get; set; }

        public byte[]? Documento { get; set; }

        public Tarea tarea { get; set; }

        public Factura() {
        
        }

        public Factura(string id_Factura, string nombre, int precio, UsuarioEspacio[] reparto, bool pagado, byte[]? documento)
        {
            Id_Factura = id_Factura;
            Nombre = nombre;
            Precio = precio;
            Reparto = reparto;
            Pagado = pagado;
            Documento = documento;
        }
    }
}
