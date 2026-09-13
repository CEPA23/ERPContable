namespace ERPContable.Domain.Entities;

public class Compra
{
    private Compra() { }
    public int Id { get; private set; }
    public int EmpresaId { get; private set; }
    public int ProveedorId { get; private set; }
    public Proveedor? Proveedor { get; private set; }
    public DateTime Fecha { get; private set; }
    public string TipoComprobante { get; private set; } = string.Empty;
    public string Serie { get; private set; } = string.Empty;
    public string Numero { get; private set; } = string.Empty;
    public decimal Subtotal { get; private set; }
    public decimal Igv { get; private set; }
    public decimal Total { get; private set; }
    public string FormaPago { get; private set; } = string.Empty;
    public int CuentaContrapartidaId { get; private set; }
    public int CuentaIgvId { get; private set; }
    public int? AsientoContableId { get; private set; }
    public List<CompraDetalle> Detalles { get; private set; } = [];
    public List<CorreccionDocumento> Correcciones { get; private set; } = [];

    public static Compra Crear(int empresaId, int proveedorId, DateTime fecha, string tipo, string serie, string numero, decimal igv, string formaPago, int cuentaContrapartidaId, int cuentaIgvId, IEnumerable<CompraDetalle> detalles)
    {
        var items = detalles.ToList();
        var subtotal = items.Sum(x => x.Total);
        if (empresaId <= 0 || proveedorId <= 0 || items.Count == 0 || subtotal <= 0 || igv < 0 || cuentaContrapartidaId <= 0 || cuentaIgvId <= 0)
            throw new ArgumentException("Completa los datos y agrega al menos una línea válida a la compra.");
        if (string.IsNullOrWhiteSpace(tipo) || string.IsNullOrWhiteSpace(serie) || string.IsNullOrWhiteSpace(numero))
            throw new ArgumentException("Tipo, serie y número de comprobante son obligatorios.");
        if (!string.Equals(formaPago, "CONTADO", StringComparison.OrdinalIgnoreCase) && !string.Equals(formaPago, "CREDITO", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("La forma de pago debe ser CONTADO o CREDITO.");
        var compra = new Compra { EmpresaId = empresaId, ProveedorId = proveedorId, Fecha = fecha.Date, TipoComprobante = tipo.Trim().ToUpperInvariant(), Serie = serie.Trim(), Numero = numero.Trim(), Subtotal = subtotal, Igv = decimal.Round(igv, 2), Total = decimal.Round(subtotal + igv, 2), FormaPago = formaPago.Trim().ToUpperInvariant(), CuentaContrapartidaId = cuentaContrapartidaId, CuentaIgvId = cuentaIgvId, Detalles = items };
        return compra;
    }

    public void AsignarAsiento(int asientoId) => AsientoContableId = asientoId;
}

public class CompraDetalle
{
    private CompraDetalle() { }
    public int Id { get; private set; }
    public int CompraId { get; private set; }
    public int CuentaContableId { get; private set; }
    public CuentaContable? CuentaContable { get; private set; }
    public int? ProductoServicioId { get; private set; }
    public ProductoServicio? ProductoServicio { get; private set; }
    public string Descripcion { get; private set; } = string.Empty;
    public decimal Cantidad { get; private set; }
    public decimal PrecioUnitario { get; private set; }
    public decimal Total { get; private set; }

    public static CompraDetalle Crear(int cuentaId, string descripcion, decimal cantidad, decimal precioUnitario, int? productoServicioId = null)
    {
        if (cuentaId <= 0 || string.IsNullOrWhiteSpace(descripcion) || cantidad <= 0 || precioUnitario < 0)
            throw new ArgumentException("La línea de compra no es válida.");
        return new CompraDetalle { CuentaContableId = cuentaId, ProductoServicioId = productoServicioId, Descripcion = descripcion.Trim(), Cantidad = cantidad, PrecioUnitario = precioUnitario, Total = decimal.Round(cantidad * precioUnitario, 2) };
    }
}
