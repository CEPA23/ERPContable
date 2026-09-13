using ERPContable.Application.Dtos;
using ERPContable.Application.Interfaces;
using ERPContable.Domain.Entities;
using ERPContable.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPContable.Infrastructure.Services;

public sealed class ComprasService : IComprasService
{
    private readonly ERPDbContext _db;
    private readonly ICierreContableService _cierres;
    private readonly IInventarioService _inventario;
    public ComprasService(ERPDbContext db, ICierreContableService cierres, IInventarioService inventario) { _db = db; _cierres = cierres; _inventario = inventario; }

    public async Task<IReadOnlyList<ProveedorDto>> ObtenerProveedoresAsync(int empresaId, CancellationToken ct = default)
        => await _db.Proveedores.AsNoTracking().Where(x => x.EmpresaId == empresaId && x.Activo).OrderBy(x => x.RazonSocial).Select(x => new ProveedorDto(x.Id, x.Documento, x.RazonSocial, x.Direccion, x.Email, x.Activo)).ToListAsync(ct);

    public async Task<ProveedorDto> CrearProveedorAsync(int empresaId, ProveedorCreateDto request, CancellationToken ct = default)
    {
        if (!await _db.Empresas.AnyAsync(x => x.Id == empresaId, ct)) throw new ArgumentException("La empresa indicada no existe.");
        if (await _db.Proveedores.AnyAsync(x => x.EmpresaId == empresaId && x.Documento == request.Documento, ct)) throw new ArgumentException("Ya existe un proveedor con ese documento.");
        var entity = Proveedor.Crear(empresaId, request.Documento, request.RazonSocial, request.Direccion, request.Email);
        _db.Proveedores.Add(entity); await _db.SaveChangesAsync(ct);
        return new ProveedorDto(entity.Id, entity.Documento, entity.RazonSocial, entity.Direccion, entity.Email, entity.Activo);
    }

    public async Task<IReadOnlyList<CompraDto>> ObtenerComprasAsync(int empresaId, CancellationToken ct = default)
    {
        var items = await _db.Compras.AsNoTracking().Include(x => x.Correcciones).Include(x => x.Proveedor).Include(x => x.Detalles).ThenInclude(x => x.CuentaContable)
            .Include(x => x.Detalles).ThenInclude(x => x.ProductoServicio)
            .Where(x => x.EmpresaId == empresaId).OrderByDescending(x => x.Fecha).ThenByDescending(x => x.Id).ToListAsync(ct);
        return items.Select(Map).ToList();
    }

    public async Task<CompraDto> CrearCompraAsync(int empresaId, CompraCreateDto request, CancellationToken ct = default)
    {
        await using var transaction = _db.Database.IsRelational() ? await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct) : null;
        await _cierres.VerificarPeriodoAbiertoAsync(empresaId, request.Fecha, ct);
        if (!await _db.Empresas.AnyAsync(x => x.Id == empresaId, ct)) throw new ArgumentException("La empresa indicada no existe.");
        var proveedor = await _db.Proveedores.FirstOrDefaultAsync(x => x.Id == request.ProveedorId && x.EmpresaId == empresaId && x.Activo, ct) ?? throw new ArgumentException("El proveedor no existe o está inactivo.");
        var ids = request.Detalles.Select(x => x.CuentaContableId).Append(request.CuentaContrapartidaId).Append(request.CuentaIgvId).Distinct().ToList();
        var cuentas = await _db.CuentasContables.Where(x => x.EmpresaId == empresaId && ids.Contains(x.Id) && x.Activa).ToListAsync(ct);
        if (cuentas.Count != ids.Count) throw new ArgumentException("Una o más cuentas contables no existen o están inactivas.");
        var productoIds = request.Detalles.Where(x => x.ProductoServicioId.HasValue).Select(x => x.ProductoServicioId!.Value).Distinct().ToList();
        var productos = await _db.ProductosServicios.Where(x => x.EmpresaId == empresaId && x.Tipo == "PRODUCTO" && x.Activo && productoIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
        if (productos.Count != productoIds.Count) throw new ArgumentException("Uno o más productos de inventario no existen o están inactivos.");
        var accountingConfig = productoIds.Count == 0
            ? null
            : await _db.ConfiguracionesContablesEmpresas.AsNoTracking().FirstOrDefaultAsync(x => x.EmpresaId == empresaId, ct);
        var cuentaInventario = accountingConfig?.CuentaInventarioId is int inventoryId
            ? await _db.CuentasContables.FirstOrDefaultAsync(x => x.Id == inventoryId && x.EmpresaId == empresaId && x.Activa && x.Tipo == "ACTIVO", ct)
            : null;
        if (productoIds.Count > 0 && cuentaInventario is null) throw new ArgumentException("Configura una cuenta activa de tipo ACTIVO para el inventario antes de registrar la compra.");
        var detalles = request.Detalles.Select(x => CompraDetalle.Crear(x.ProductoServicioId.HasValue ? cuentaInventario!.Id : x.CuentaContableId, x.Descripcion, x.Cantidad, x.PrecioUnitario, x.ProductoServicioId)).ToList();
        var compra = Compra.Crear(empresaId, proveedor.Id, request.Fecha, request.TipoComprobante, request.Serie, request.Numero, request.Igv, request.FormaPago, request.CuentaContrapartidaId, request.CuentaIgvId, detalles);
        var asientoLineas = detalles.GroupBy(x => x.CuentaContableId).Select(x => (x.Key, x.Sum(y => y.Total), 0m)).ToList();
        if (compra.Igv > 0) asientoLineas.Add((compra.CuentaIgvId, compra.Igv, 0m));
        asientoLineas.Add((compra.CuentaContrapartidaId, 0m, compra.Total));
        var asiento = AsientoContable.Crear(empresaId, compra.Fecha, $"Compra {compra.TipoComprobante} {compra.Serie}-{compra.Numero} - {proveedor.RazonSocial}", asientoLineas);
        _db.Compras.Add(compra); _db.AsientosContables.Add(asiento);
        await _db.SaveChangesAsync(ct); compra.AsignarAsiento(asiento.Id); await _db.SaveChangesAsync(ct);
        foreach (var detalle in detalles.Where(x => x.ProductoServicioId.HasValue))
            await _inventario.RegistrarMovimientoAsync(empresaId, new MovimientoInventarioCreateDto(detalle.ProductoServicioId!.Value, "ENTRADA", detalle.Cantidad, null, detalle.PrecioUnitario, $"{compra.TipoComprobante} {compra.Serie}-{compra.Numero}", "Entrada automática por compra", request.Fecha), ct);
        await _db.Entry(compra).Reference(x => x.Proveedor).LoadAsync(ct); await _db.Entry(compra).Collection(x => x.Detalles).Query().Include(x => x.CuentaContable).Include(x => x.ProductoServicio).LoadAsync(ct);
        if (transaction is not null) await transaction.CommitAsync(ct);
        return Map(compra);
    }

    private static CompraDto Map(Compra x) => new(x.Id, x.Fecha, x.Proveedor?.RazonSocial ?? string.Empty, $"{x.TipoComprobante} {x.Serie}-{x.Numero}", x.Subtotal, x.Igv, x.Total, x.FormaPago, x.AsientoContableId, x.Detalles.Select(d => new CompraDetalleDto($"{d.CuentaContable?.Codigo} - {d.CuentaContable?.Nombre}", d.Descripcion, d.Cantidad, d.PrecioUnitario, d.Total, d.ProductoServicioId, d.ProductoServicio?.Nombre)).ToList(), CorreccionesService.Estado(x.Correcciones), x.Correcciones.Sum(c => c.Subtotal), x.Correcciones.Sum(c => c.Igv));
}
