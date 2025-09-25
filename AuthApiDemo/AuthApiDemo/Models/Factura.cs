namespace AuthApiDemo.Models
{
    /// <summary>
    /// Representa una factura con reparto de pago entre usuarios, estado de pago y posible documento/tarea asociada.
    /// </summary>
    public class Factura
    {
        /// <summary>
        /// Identificador único de la factura.
        /// </summary>
        public string Id_Factura { get; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Nombre o concepto de la factura.
        /// </summary>
        public string Nombre { get; set; }

        /// <summary>
        /// Importe total de la factura.
        /// </summary>
        public float Precio { get; set; }

        /// <summary>
        /// Mapa de reparto: cada usuario y la cantidad que debe pagar. Solo lectura desde fuera.
        /// </summary>
        private Dictionary<UsuarioEspacio, float> _repartoMap = new Dictionary<UsuarioEspacio, float>();
        public IReadOnlyDictionary<UsuarioEspacio, float> RepartoMap => _repartoMap;

        /// <summary>
        /// Indica si la factura está pagada completamente.
        /// </summary>
        public bool Pagado { get; set; }

        /// <summary>
        /// Documento o imagen asociada a la factura (opcional).
        /// </summary>
        public byte[]? Documento { get; set; }

        /// <summary>
        /// Tarea asociada a la factura (opcional).
        /// </summary>
        public Tarea? Tarea { get; set; }

        /// <summary>
        /// Constructor por defecto para deserialización o pruebas.
        /// </summary>
        public Factura() { }

        /// <summary>
        /// Crea una factura con reparto igualitario entre usuarios.
        /// </summary>
        public Factura(string id_Factura, string nombre, float precio, List<UsuarioEspacio> reparto, bool pagado, byte[]? documento)
        {
            if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre no puede estar vacío.", nameof(nombre));
            if (precio <= 0) throw new ArgumentException("El precio debe ser positivo.", nameof(precio));
            if (reparto == null || reparto.Count == 0) throw new ArgumentException("El reparto no puede estar vacío.", nameof(reparto));

            Id_Factura = id_Factura;
            Nombre = nombre;
            Precio = precio;
            Pagado = pagado;
            Documento = documento;
            foreach (var usuario in reparto)
            {
                    _repartoMap[usuario] = precio / reparto.Count;
            }
        }

        /// <summary>
        /// Crea una factura con reparto específico entre usuarios.
        /// </summary>
        public Factura(string id_Factura, string nombre, float precio, Dictionary<UsuarioEspacio, float> reparto, bool pagado, byte[]? documento)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre no puede estar vacío.", nameof(nombre));
            if (precio <= 0)
                throw new ArgumentException("El precio debe ser positivo.", nameof(precio));
            if (reparto == null || reparto.Count == 0)
                throw new ArgumentException("El reparto no puede estar vacío.", nameof(reparto));

            Id_Factura = id_Factura;
            Nombre = nombre;
            Precio = precio;
            Pagado = pagado;
            Documento = documento;
            _repartoMap = new Dictionary<UsuarioEspacio, float>(reparto);
        }

        /// <summary>
        /// Crea una factura con reparto igualitario y tarea asociada.
        /// </summary>
        public Factura(string id_Factura, string nombre, float precio, List<UsuarioEspacio> reparto, bool pagado, byte[]? documento, Tarea tarea)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre no puede estar vacío.", nameof(nombre));
            if (precio <= 0)
                throw new ArgumentException("El precio debe ser positivo.", nameof(precio));
            if (reparto == null || reparto.Count == 0)
                throw new ArgumentException("El reparto no puede estar vacío.", nameof(reparto));

            Id_Factura = id_Factura;
            Nombre = nombre;
            Precio = precio;
            Pagado = pagado;
            Documento = documento;
            Tarea = tarea;
            foreach (var usuario in reparto)
            {
                _repartoMap[usuario] = precio / reparto.Count;
            }
        }

        /// <summary>
        /// Crea una factura con reparto específico y tarea asociada.
        /// </summary>
        public Factura(string id_Factura, string nombre, float precio, Dictionary<UsuarioEspacio, float> reparto, bool pagado, byte[]? documento, Tarea tarea)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre no puede estar vacío.", nameof(nombre));
            if (precio <= 0)
                throw new ArgumentException("El precio debe ser positivo.", nameof(precio));
            if (reparto == null || reparto.Count == 0)
                throw new ArgumentException("El reparto no puede estar vacío.", nameof(reparto));

            Id_Factura = id_Factura;
            Nombre = nombre;
            Precio = precio;
            Pagado = pagado;
            Documento = documento;
            _repartoMap = new Dictionary<UsuarioEspacio, float>(reparto);
            Tarea = tarea;
        }

        /// <summary>
        /// Edita los datos principales de la factura y su reparto.
        /// </summary>
        public void EditarFactura(string nombre, float precio, Dictionary<UsuarioEspacio, float> reparto, bool pagado, byte[]? documento = null)
        {
            if (Pagado)
                throw new InvalidOperationException("No se puede editar una factura ya pagada.");

            Nombre = nombre;
            Precio = precio;
            _repartoMap = reparto;
            Pagado = pagado;
            if (documento != null)
            {
                Documento = documento;
            }
        }

        /// <summary>
        /// Adjunta un documento o imagen a la factura.
        /// </summary>
        public void AdjuntarDocumento(byte[] documento)
        {
            Documento = documento;
        }

        /// <summary>
        /// Elimina el documento asociado a la factura.
        /// </summary>
        public void EliminarDocumento()
        {
            Documento = null;
        }

        /// <summary>
        /// Marca la factura como pagada.
        /// </summary>
        public void EsPagada()
        {
            Pagado = true;
        }

        /// <summary>
        /// Realiza un pago parcial de un usuario. Si la deuda queda saldada, se elimina del reparto.
        /// </summary>
        public bool Pagar(UsuarioEspacio usuario, float cantidad)
        {
            if (!_repartoMap.ContainsKey(usuario))
                return false;
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser positiva.", nameof(cantidad));

            _repartoMap[usuario] -= cantidad;
            if (_repartoMap[usuario] <= 0)
                _repartoMap.Remove(usuario);
            return true;
        }
    }
}
