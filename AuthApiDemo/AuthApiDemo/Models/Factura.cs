namespace AuthApiDemo.Models
{
    public class Factura
    {

        public string Id_Factura { get; set; } = Guid.NewGuid().ToString();

        public string Nombre { get; set; }

        public float Precio { get; set; }

        public Dictionary<UsuarioEspacio, float> RepartoMap { get; set; }

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
            Pagado = pagado;
            Documento = documento;
            RepartoMap = new Dictionary<UsuarioEspacio, float>();
            foreach (var usuario in reparto)
            {
                RepartoMap[usuario] = precio / reparto.Count; // Reparto igualitario por defecto
            }
        }
        public Factura(string id_Factura, string nombre, int precio, Dictionary<UsuarioEspacio, float> reparto, bool pagado, byte[]? documento)
        {
            Id_Factura = id_Factura;
            Nombre = nombre;
            Precio = precio;
            Pagado = pagado;
            Documento = documento;
            this.RepartoMap = reparto;
        }


        public Factura(string id_Factura, string nombre, int precio, List<UsuarioEspacio> reparto, bool pagado, byte[]? documento, Tarea tarea)
        {
            Id_Factura = id_Factura;
            Nombre = nombre;
            Precio = precio;
            Pagado = pagado;
            Documento = documento;
            RepartoMap = new Dictionary<UsuarioEspacio, float>();
            this.tarea = tarea;
            foreach (var usuario in reparto)
            {
                RepartoMap[usuario] = precio / reparto.Count; // Reparto igualitario por defecto
            }
        }
        public Factura(string id_Factura, string nombre, int precio, Dictionary<UsuarioEspacio, float> reparto, bool pagado, byte[]? documento, Tarea tarea)
        {
            Id_Factura = id_Factura;
            Nombre = nombre;
            Precio = precio;
            Pagado = pagado;
            Documento = documento;
            this.RepartoMap = reparto;
            this.tarea = tarea;
        }
        public void editarFactura(string nombre, int precio, Dictionary<UsuarioEspacio, float> reparto, bool pagado, byte[]? documento = null)
        {
            Nombre = nombre;
            Precio = precio;
            RepartoMap = reparto;
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

        public void pagar(UsuarioEspacio usuario, float cantidad)
        {
            if(RepartoMap.ContainsKey(usuario))
            {
                RepartoMap[usuario] -= cantidad;
                if(RepartoMap[usuario] <= 0)
                {
                    RepartoMap.Remove(usuario);
                }
            }
        }

    }
}
