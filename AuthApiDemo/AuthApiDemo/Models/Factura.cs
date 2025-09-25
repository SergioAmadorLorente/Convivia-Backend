namespace AuthApiDemo.Models
{
    public class Factura
    {
        // Atributos de la case facutra
        public string Id_Factura { get; set; } = Guid.NewGuid().ToString();

        public string Nombre { get; set; }

        public float Precio { get; set; }

        // Mapa de reparto: UsuarioEspacio -> cantidad a pagar
        public Dictionary<UsuarioEspacio, float> RepartoMap { get; set; }

        public bool Pagado { get; set; }
        // Imagen o documento asociado a la factura
        public byte[]? Documento { get; set; }

        public Tarea tarea { get; set; }

        public Factura()
        {

        }
        // Hay 4 constructores posibles, con o sin tarea asociada y con reparto como lista o como diccionario
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
            this.RepartoMap = reparto; // reparto especifico
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
            this.RepartoMap = reparto; // reparto especifico
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
        // Cuanto un usuario paga, se le descuenta de su deuda en el mapa de reparto, cuando no tiene deuda se elimina del mapa
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
