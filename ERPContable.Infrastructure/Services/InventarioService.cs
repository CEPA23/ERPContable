using ERPContable.Application.Dtos;
using ERPContable.Application.Interfaces;
using ERPContable.Domain.Entities;
using ERPContable.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPContable.Infrastructure.Services;

public sealed class InventarioService : IInventarioService
{
    private readonly ERPDbContext _db;
    private readonly ICierreContableService _cierres;
    public InventarioService(ERPDbContext db, ICierreContableService cierres) { _db = db; _cierres = cierres; }

    public async Task<InventarioDto> ObtenerAsync(int empresaId, int? productoId = null, CancellationToken ct = default)
    {
        await EnsureCompany(empresaId, ct);
        var productosQuery = _db.ProductosServicios.AsNoTracking().Include(x => x.UnidadMedida).Where(x => x.EmpresaId == empresaId && x.Tipo == "PRODUCTO");
        if (productoId is not null) productosQuery = productosQuery.Where(x => x.Id == productoId);
        var productos = await productosQuery.OrderBy(x => x.Nombre).ToListAsync(ct);
        var ids = productos.Select(x => x.Id).ToArray();
        var stocks = await _db.InventarioProductos.AsNoTracking().Where(x => x.EmpresaId == empresaId && ids.Contains(x.ProductoServicioId)).ToDictionaryAsync(x => x.ProductoServicioId, ct);
        var movimientos = await _db.MovimientosInventario.AsNoTracking().Include(x => x.ProductoServicio).Where(x => x.EmpresaId == empresaId && (productoId == null || x.ProductoServicioId == productoId)).OrderByDescending(x => x.FechaUtc).Take(200).ToListAsync(ct);
        var stockDtos = productos.Select(x => stocks.TryGetValue(x.Id, out var stock) ? Map(stock, x) : new InventarioProductoDto(0, x.Id, x.Codigo, x.Nombre, x.UnidadMedida?.Abreviatura ?? "", 0, 0, 0, false)).ToList();
        return new InventarioDto(stockDtos, movimientos.Select(Map).ToList());
    }

    public async Task<MovimientoInventarioDto> RegistrarMovimientoAsync(int empresaId, MovimientoInventarioCreateDto request, CancellationToken ct = default)
    {
        await using var transaction = _db.Database.IsRelational() && _db.Database.CurrentTransaction is null
            ? await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct) : null;
        await EnsureCompany(empresaId, ct);
        var producto = await _db.ProductosServicios.Include(x => x.UnidadMedida).SingleOrDefaultAsync(x => x.Id == request.ProductoId && x.EmpresaId == empresaId && x.Tipo == "PRODUCTO" && x.Activo, ct);
        if (producto is null) throw new ArgumentException("El producto no existe, está inactivo o no controla inventario.");
        if (request.CostoUnitario < 0) throw new ArgumentException("El costo unitario no puede ser negativo.");
        var tipo = request.Tipo.Trim().ToUpperInvariant();
        var fecha = request.Fecha.HasValue ? DateTime.SpecifyKind(request.Fecha.Value.Date, DateTimeKind.Utc) : DateTime.UtcNow;
        await _cierres.VerificarPeriodoAbiertoAsync(empresaId, fecha, ct);
        var stock = await _db.InventarioProductos.SingleOrDefaultAsync(x => x.EmpresaId == empresaId && x.ProductoServicioId == producto.Id, ct);
        stock ??= InventarioProducto.Crear(empresaId, producto.Id);
        var anterior = stock.StockActual;
        decimal delta;
        if (tipo == "AJUSTE")
        {
            if (request.StockObjetivo is null || request.StockObjetivo < 0) throw new ArgumentException("El ajuste requiere un stock objetivo válido.");
            delta = request.StockObjetivo.Value - anterior;
            if (delta == 0) throw new ArgumentException("El stock objetivo ya coincide con el stock actual.");
        }
        else if (tipo is "ENTRADA" or "SALIDA")
        {
            if (request.Cantidad <= 0) throw new ArgumentException("La cantidad debe ser mayor que cero.");
            delta = tipo == "ENTRADA" ? request.Cantidad : -request.Cantidad;
        }
        else throw new ArgumentException("El tipo debe ser ENTRADA, SALIDA o AJUSTE.");
        var costoMovimiento = request.CostoUnitario > 0 ? request.CostoUnitario : tipo == "SALIDA" ? stock.CostoPromedio : producto.CostoReferencial;
        stock.AplicarMovimiento(delta, costoMovimiento);
        var movimiento = MovimientoInventario.Crear(empresaId, producto.Id, tipo, Math.Abs(delta), costoMovimiento, anterior, stock.StockActual, request.Referencia, request.Observacion, fecha);
        if (stock.Id == 0) _db.InventarioProductos.Add(stock);
        _db.MovimientosInventario.Add(movimiento);
        await _db.SaveChangesAsync(ct);
        if (transaction is not null) await transaction.CommitAsync(ct);
        return Map(movimiento);
    }

    public async Task<InventarioProductoDto> ActualizarStockMinimoAsync(int empresaId, int productoId, StockMinimoUpdateDto request, CancellationToken ct = default)
    {
        await EnsureCompany(empresaId, ct);
        var producto = await _db.ProductosServicios.Include(x => x.UnidadMedida).SingleOrDefaultAsync(x => x.Id == productoId && x.EmpresaId == empresaId && x.Tipo == "PRODUCTO" && x.Activo, ct);
        if (producto is null) throw new ArgumentException("El producto no existe o no controla inventario.");
        var stock = await _db.InventarioProductos.SingleOrDefaultAsync(x => x.EmpresaId == empresaId && x.ProductoServicioId == productoId, ct);
        stock ??= InventarioProducto.Crear(empresaId, productoId);
        stock.ActualizarStockMinimo(request.StockMinimo);
        if (stock.Id == 0) _db.InventarioProductos.Add(stock);
        await _db.SaveChangesAsync(ct);
        return Map(stock, producto);
    }

    private async Task EnsureCompany(int empresaId, CancellationToken ct) { if (!await _db.Empresas.AnyAsync(x => x.Id == empresaId && x.Activa, ct)) throw new ArgumentException("La empresa indicada no existe o está inactiva."); }
    private static InventarioProductoDto Map(InventarioProducto x, ProductoServicio p) => new(x.Id, p.Id, p.Codigo, p.Nombre, p.UnidadMedida?.Abreviatura ?? "", x.StockActual, x.StockMinimo, x.CostoPromedio, x.StockActual <= x.StockMinimo);
    private static MovimientoInventarioDto Map(MovimientoInventario x) => new(x.Id, x.ProductoServicioId, x.ProductoServicio?.Codigo ?? "", x.ProductoServicio?.Nombre ?? "", x.Tipo, x.Cantidad, x.CostoUnitario, x.StockAnterior, x.StockPosterior, x.Referencia, x.Observacion, x.FechaUtc);
}
