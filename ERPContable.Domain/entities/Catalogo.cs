namespace ERPContable.Domain.Entities;

public sealed class CategoriaProducto
{
    private CategoriaProducto() { }
    public int Id { get; private set; }
    public int EmpresaId { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public string? Descripcion { get; private set; }
    public bool Activa { get; private set; } = true;
    public DateTime CreadaEnUtc { get; private set; }

    public static CategoriaProducto Crear(int empresaId, string nombre, string? descripcion = null)
    {
        if (empresaId <= 0 || string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("La categoría requiere empresa y nombre.");
        return new CategoriaProducto { EmpresaId = empresaId, Nombre = nombre.Trim(), Descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim(), CreadaEnUtc = DateTime.UtcNow };
    }
}

public sealed class UnidadMedida
{
    private UnidadMedida() { }
    public int Id { get; private set; }
    public int EmpresaId { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public string Abreviatura { get; private set; } = string.Empty;
    public bool Activa { get; private set; } = true;

    public static UnidadMedida Crear(int empresaId, string codigo, string nombre, string abreviatura)
    {
        if (empresaId <= 0 || string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(abreviatura)) throw new ArgumentException("La unidad requiere código, nombre y abreviatura.");
        return new UnidadMedida { EmpresaId = empresaId, Codigo = codigo.Trim().ToUpperInvariant(), Nombre = nombre.Trim(), Abreviatura = abreviatura.Trim().ToUpperInvariant() };
    }
}

public sealed class Impuesto
{
    private Impuesto() { }
    public int Id { get; private set; }
    public int EmpresaId { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public decimal Tasa { get; private set; }
    public bool Activa { get; private set; } = true;

    public static Impuesto Crear(int empresaId, string codigo, string nombre, decimal tasa)
    {
        if (empresaId <= 0 || string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(nombre) || tasa < 0 || tasa > 100) throw new ArgumentException("El impuesto requiere código, nombre y una tasa entre 0 y 100.");
        return new Impuesto { EmpresaId = empresaId, Codigo = codigo.Trim().ToUpperInvariant(), Nombre = nombre.Trim(), Tasa = decimal.Round(tasa, 2) };
    }
}

public sealed class ProductoServicio
{
    private ProductoServicio() { }
    public int Id { get; private set; }
    public int EmpresaId { get; private set; }
    public string Tipo { get; private set; } = "PRODUCTO";
    public string Codigo { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public string? Descripcion { get; private set; }
    public int? CategoriaId { get; private set; }
    public int UnidadMedidaId { get; private set; }
    public int ImpuestoId { get; private set; }
    public decimal PrecioVenta { get; private set; }
    public decimal CostoReferencial { get; private set; }
    public bool Activo { get; private set; } = true;
    public DateTime CreadoEnUtc { get; private set; }
    public CategoriaProducto? Categoria { get; private set; }
    public UnidadMedida? UnidadMedida { get; private set; }
    public Impuesto? Impuesto { get; private set; }

    public static ProductoServicio Crear(int empresaId, string tipo, string codigo, string nombre, string? descripcion, int? categoriaId, int unidadMedidaId, int impuestoId, decimal precioVenta, decimal costoReferencial)
    {
        var tipoNormalizado = tipo.Trim().ToUpperInvariant();
        if (empresaId <= 0 || (tipoNormalizado != "PRODUCTO" && tipoNormalizado != "SERVICIO") || string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(nombre) || unidadMedidaId <= 0 || impuestoId <= 0 || precioVenta < 0 || costoReferencial < 0)
            throw new ArgumentException("Completa el tipo, código, nombre, unidad, impuesto y precios válidos.");
        return new ProductoServicio { EmpresaId = empresaId, Tipo = tipoNormalizado, Codigo = codigo.Trim().ToUpperInvariant(), Nombre = nombre.Trim(), Descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim(), CategoriaId = categoriaId, UnidadMedidaId = unidadMedidaId, ImpuestoId = impuestoId, PrecioVenta = decimal.Round(precioVenta, 2), CostoReferencial = decimal.Round(costoReferencial, 2), CreadoEnUtc = DateTime.UtcNow };
    }
}

public sealed class CentroCosto
{
    private CentroCosto() { }
    public int Id { get; private set; }
    public int EmpresaId { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public string? Descripcion { get; private set; }
    public bool Activo { get; private set; } = true;
    public DateTime CreadoEnUtc { get; private set; }

    public static CentroCosto Crear(int empresaId, string codigo, string nombre, string? descripcion = null)
    {
        if (empresaId <= 0 || string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El centro de costo requiere código y nombre.");
        return new CentroCosto { EmpresaId = empresaId, Codigo = codigo.Trim().ToUpperInvariant(), Nombre = nombre.Trim(), Descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim(), CreadoEnUtc = DateTime.UtcNow };
    }
}

public sealed class InventarioProducto
{
    private InventarioProducto() { }
    public int Id { get; private set; }
    public int EmpresaId { get; private set; }
    public int ProductoServicioId { get; private set; }
    public decimal StockActual { get; private set; }
    public decimal StockMinimo { get; private set; }
    public decimal CostoPromedio { get; private set; }
    public bool Activo { get; private set; } = true;
    public ProductoServicio? ProductoServicio { get; private set; }

    public static InventarioProducto Crear(int empresaId, int productoServicioId, decimal stockMinimo = 0)
    {
        if (empresaId <= 0 || productoServicioId <= 0 || stockMinimo < 0) throw new ArgumentException("El inventario requiere un producto y un stock mínimo válido.");
        return new InventarioProducto { EmpresaId = empresaId, ProductoServicioId = productoServicioId, StockMinimo = decimal.Round(stockMinimo, 4) };
    }

    public void AplicarMovimiento(decimal delta, decimal costoUnitario)
    {
        var nuevoStock = StockActual + delta;
        if (nuevoStock < 0) throw new ArgumentException("La salida supera el stock disponible.");
        if (delta > 0 && costoUnitario > 0)
        {
            var valorAnterior = StockActual * CostoPromedio;
            CostoPromedio = decimal.Round((valorAnterior + delta * costoUnitario) / nuevoStock, 4);
        }
        StockActual = decimal.Round(nuevoStock, 4);
    }

    public void ActualizarStockMinimo(decimal stockMinimo)
    {
        if (stockMinimo < 0) throw new ArgumentException("El stock mínimo no puede ser negativo.");
        StockMinimo = decimal.Round(stockMinimo, 4);
    }

    public void RevertirInventario(decimal delta, decimal valor)
    {
        var cantidad = StockActual + delta;
        var saldo = StockActual * CostoPromedio + valor;
        if (cantidad < 0) throw new ArgumentException("No hay stock suficiente para devolver esta compra.");
        if (saldo < -0.01m || (cantidad == 0 && Math.Abs(saldo) > 0.01m))
            throw new ArgumentException("El valor disponible no permite revertir la compra a su costo original. Revisa los movimientos posteriores del producto.");
        StockActual = cantidad;
        CostoPromedio = cantidad == 0 ? 0 : decimal.Round(Math.Max(0, saldo) / cantidad, 4);
    }
}

public sealed class MovimientoInventario
{
    private MovimientoInventario() { }
    public int Id { get; private set; }
    public int EmpresaId { get; private set; }
    public int ProductoServicioId { get; private set; }
    public string Tipo { get; private set; } = string.Empty;
    public decimal Cantidad { get; private set; }
    public decimal CostoUnitario { get; private set; }
    public decimal StockAnterior { get; private set; }
    public decimal StockPosterior { get; private set; }
    public string? Referencia { get; private set; }
    public string? Observacion { get; private set; }
    public DateTime FechaUtc { get; private set; }
    public ProductoServicio? ProductoServicio { get; private set; }

    public static MovimientoInventario Crear(int empresaId, int productoServicioId, string tipo, decimal cantidad, decimal costoUnitario, decimal stockAnterior, decimal stockPosterior, string? referencia, string? observacion, DateTime? fechaUtc = null)
    {
        var tipoNormalizado = tipo.Trim().ToUpperInvariant();
        if (empresaId <= 0 || productoServicioId <= 0 || !new[] { "ENTRADA", "SALIDA", "AJUSTE" }.Contains(tipoNormalizado) || cantidad <= 0 || costoUnitario < 0 || stockAnterior < 0 || stockPosterior < 0)
            throw new ArgumentException("El movimiento de inventario no tiene datos válidos.");
        var fecha = fechaUtc.HasValue ? DateTime.SpecifyKind(fechaUtc.Value, DateTimeKind.Utc) : DateTime.UtcNow;
        return new MovimientoInventario { EmpresaId = empresaId, ProductoServicioId = productoServicioId, Tipo = tipoNormalizado, Cantidad = decimal.Round(Math.Abs(cantidad), 4), CostoUnitario = decimal.Round(costoUnitario, 4), StockAnterior = decimal.Round(stockAnterior, 4), StockPosterior = decimal.Round(stockPosterior, 4), Referencia = string.IsNullOrWhiteSpace(referencia) ? null : referencia.Trim(), Observacion = string.IsNullOrWhiteSpace(observacion) ? null : observacion.Trim(), FechaUtc = fecha };
    }
}
