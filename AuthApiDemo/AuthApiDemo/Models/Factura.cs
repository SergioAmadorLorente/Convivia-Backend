namespace AuthApiDemo.Models
{
    public class Factura
    {

        public string Id_Factura { get; set; } = Guid.NewGuid().ToString();

        public string Nombre { get; set; }

        public int Precio { get; set; }

        public List<UsuarioEspacio> Reparto { get; set; }

        public bool Pagado { get; set; }

        public byte[]? Documento { get; set; }

        public Tarea tarea { get; set; }

        public Factura()
        {

        }

        public Factura(string id_Factura, string nombre, int precio, List<UsuarioEspacio> reparto, bool pagado, byte[]? documento)
        {
            Id_Factura = id_Factura;
            Nombre = nombre;
            Precio = precio;
            Reparto = reparto;
            Pagado = pagado;
            Documento = documento;
        }

        public void editarFactura(string nombre, int precio, List<UsuarioEspacio> reparto, bool pagado, byte[]? documento = null)
        {
            Nombre = nombre;
            Precio = precio;
            Reparto = reparto;
            Pagado = pagado;
            if (documento != null)
            {
                Documento = documento;
            }
        }

        public void adjuntarDocumento(byte[] documento)
        {
            Documento = documento;
        }

        public void eliminarDocumento()
        {
            Documento = null;
        }

        public void esPagada()
        {
            Pagado = true;
        }

        public void pagar(UsuarioEspacio usuario, int cantidad)
        {

            Reparto.ToList().Remove(usuario);
            Precio -= cantidad;

            if (Reparto.Count == 0)
            {
                esPagada();
            }
        }

    }
}
