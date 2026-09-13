using ERPContable.Application.Dtos;
using ERPContable.Application.Interfaces;
using ERPContable.Domain.Entities;
using ERPContable.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPContable.Infrastructure.Services;

public sealed class VentasService : IVentasService
{
    private readonly ERPDbContext _db;
    private readonly ICierreContableService _cierres;
    private readonly IInventarioService _inventario;

    public VentasService(ERPDbContext db, ICierreContableService cierres, IInventarioService inventario)
    {
        _db = db;
        _cierres = cierres;
        _inventario = inventario;
    }

    public async Task<IReadOnlyList<ClienteDto>> ObtenerClientesAsync(int empresaId, CancellationToken ct = default)
        => await _db.Clientes.AsNoTracking().Where(x => x.EmpresaId == empresaId && x.Activo).OrderBy(x => x.RazonSocial)
            .Select(x => new ClienteDto(x.Id, x.Documento, x.RazonSocial, x.Direccion, x.Email, x.Activo)).ToListAsync(ct);

    public async Task<ClienteDto> CrearClienteAsync(int empresaId, ClienteCreateDto request, CancellationToken ct = default)
    {
        if (!await _db.Empresas.AnyAsync(x => x.Id == empresaId, ct)) throw new ArgumentException("La empresa indicada no existe.");
        if (await _db.Clientes.AnyAsync(x => x.EmpresaId == empresaId && x.Documento == request.Documento, ct)) throw new ArgumentException("Ya existe un cliente con ese documento.");
        var entity = Cliente.Crear(empresaId, request.Documento, request.RazonSocial, request.Direccion, request.Email);
        _db.Clientes.Add(entity);
        await _db.SaveChangesAsync(ct);
        return new ClienteDto(entity.Id, entity.Documento, entity.RazonSocial, entity.Direccion, entity.Email, entity.Activo);
    }

    public async Task<IReadOnlyList<VentaDto>> ObtenerVentasAsync(int empresaId, CancellationToken ct = default)
    {
        var items = await _db.Ventas.AsNoTracking().Include(x => x.Correcciones).Include(x => x.Cliente).Include(x => x.Detalles).ThenInclude(x => x.CuentaContable)
            .Include(x => x.Detalles).ThenInclude(x => x.ProductoServicio).Where(x => x.EmpresaId == empresaId)
            .OrderByDescending(x => x.Fecha).ThenByDescending(x => x.Id).ToListAsync(ct);
        return items.Select(Map).ToList();
    }

    public async Task<VentaDto> CrearVentaAsync(int empresaId, VentaCreateDto request, CancellationToken ct = default)
    {
        await _cierres.VerificarPeriodoAbiertoAsync(empresaId, request.Fecha, ct);
        var transaction = _db.Database.IsRelational() ? await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct) : null;
        try
        {
            if (!await _db.Empresas.AnyAsync(x => x.Id == empresaId, ct)) throw new ArgumentException("La empresa indicada no existe.");
            var cliente = await _db.Clientes.FirstOrDefaultAsync(x => x.Id == request.ClienteId && x.EmpresaId == empresaId && x.Activo, ct)
                ?? throw new ArgumentException("El cliente no existe o está inactivo.");

            var accountIds = request.Detalles.Select(x => x.CuentaContableId).Append(request.CuentaContrapartidaId).Append(request.CuentaIgvId).Distinct().ToList();
            var cuentas = await _db.CuentasContables.Where(x => x.EmpresaId == empresaId && accountIds.Contains(x.Id) && x.Activa).ToListAsync(ct);
            if (cuentas.Count != accountIds.Count) throw new ArgumentException("Una o más cuentas contables no pertenecen a la empresa o están inactivas.");

            var productIds = request.Detalles.Where(x => x.ProductoServicioId.HasValue).Select(x => x.ProductoServicioId!.Value).Distinct().ToList();
            var products = await _db.ProductosServicios.Where(x => x.EmpresaId == empresaId && x.Tipo == "PRODUCTO" && x.Activo && productIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
            if (products.Count != productIds.Count) throw new ArgumentException("Uno o más productos de inventario no existen o están inactivos.");

            var accountingConfig = productIds.Count == 0
                ? null
                : await _db.ConfiguracionesContablesEmpresas.AsNoTracking().FirstOrDefaultAsync(x => x.EmpresaId == empresaId, ct);
            var inventoryAccount = accountingConfig?.CuentaInventarioId is int inventoryId
                ? await _db.CuentasContables.FirstOrDefaultAsync(x => x.Id == inventoryId && x.EmpresaId == empresaId && x.Activa && x.Tipo == "ACTIVO", ct)
                : null;
            var costAccount = accountingConfig?.CuentaCostoVentasId is int costId
                ? await _db.CuentasContables.FirstOrDefaultAsync(x => x.Id == costId && x.EmpresaId == empresaId && x.Activa && x.Tipo == "GASTO", ct)
                : null;
            if (productIds.Count > 0 && inventoryAccount is null) throw new ArgumentException("Configura una cuenta activa de tipo ACTIVO para el inventario antes de registrar la venta.");
            if (productIds.Count > 0 && costAccount is null) throw new ArgumentException("Configura una cuenta activa de tipo GASTO para el costo de ventas antes de registrar la venta.");

            var detalles = request.Detalles.Select(x => VentaDetalle.Crear(x.CuentaContableId, x.Descripcion, x.Cantidad, x.PrecioUnitario, x.ProductoServicioId)).ToList();
            var stockIds = detalles.Where(x => x.ProductoServicioId.HasValue).Select(x => x.ProductoServicioId!.Value).Distinct().ToList();
            var stocks = await _db.InventarioProductos.Where(x => x.EmpresaId == empresaId && stockIds.Contains(x.ProductoServicioId)).ToDictionaryAsync(x => x.ProductoServicioId, ct);
            foreach (var group in detalles.Where(x => x.ProductoServicioId.HasValue).GroupBy(x => x.ProductoServicioId!.Value))
            {
                if (!stocks.TryGetValue(group.Key, out var stock) || stock.StockActual < group.Sum(x => x.Cantidad))
                    throw new ArgumentException($"Stock insuficiente para {products[group.Key].Nombre}.");
                if (stock.CostoPromedio <= 0)
                    throw new ArgumentException($"El producto {products[group.Key].Nombre} no tiene costo promedio. Registra una entrada con costo antes de venderlo.");
            }

            var venta = Venta.Crear(empresaId, cliente.Id, request.Fecha, request.TipoComprobante, request.Serie, request.Numero, request.Igv, request.FormaPago, request.CuentaContrapartidaId, request.CuentaIgvId, detalles);
            var asientoLineas = new List<(int cuentaId, decimal debe, decimal haber)> { (venta.CuentaContrapartidaId, venta.Total, 0m) };
            asientoLineas.AddRange(detalles.GroupBy(x => x.CuentaContableId).Select(x => (x.Key, 0m, x.Sum(y => y.Total))));
            if (venta.Igv > 0) asientoLineas.Add((venta.CuentaIgvId, 0m, venta.Igv));
            var asiento = AsientoContable.Crear(empresaId, venta.Fecha, $"Venta {venta.TipoComprobante} {venta.Serie}-{venta.Numero} - {cliente.RazonSocial}", asientoLineas);
            _db.Ventas.Add(venta);
            _db.AsientosContables.Add(asiento);
            await _db.SaveChangesAsync(ct);
            venta.AsignarAsiento(asiento.Id);
            await _db.SaveChangesAsync(ct);

            decimal costoTotal = 0m;
            foreach (var group in detalles.Where(x => x.ProductoServicioId.HasValue).GroupBy(x => x.ProductoServicioId!.Value))
            {
                var movimiento = await _inventario.RegistrarMovimientoAsync(empresaId, new MovimientoInventarioCreateDto(group.Key, "SALIDA", group.Sum(x => x.Cantidad), null, 0m, $"{venta.TipoComprobante} {venta.Serie}-{venta.Numero}", "Salida automática por venta", request.Fecha), ct);
                foreach (var detalle in group)
                {
                    var anterior = decimal.Round(costoTotal, 2);
                    costoTotal += detalle.Cantidad * movimiento.CostoUnitario;
                    detalle.AsignarCostoInventario(movimiento.CostoUnitario, decimal.Round(costoTotal, 2) - anterior);
                }
            }

            if (productIds.Count > 0)
            {
                if (costoTotal <= 0) throw new ArgumentException("No se pudo determinar un costo de ventas válido para la operación.");
                var costoAsiento = AsientoContable.Crear(empresaId, venta.Fecha, $"Costo de ventas {venta.TipoComprobante} {venta.Serie}-{venta.Numero}", [(costAccount!.Id, decimal.Round(costoTotal, 2), 0m), (inventoryAccount!.Id, 0m, decimal.Round(costoTotal, 2))]);
                _db.AsientosContables.Add(costoAsiento);
                await _db.SaveChangesAsync(ct);
                venta.AsignarAsientoCostoVenta(costoAsiento.Id);
                await _db.SaveChangesAsync(ct);
            }

            await _db.Entry(venta).Reference(x => x.Cliente).LoadAsync(ct);
            await _db.Entry(venta).Collection(x => x.Detalles).Query().Include(x => x.CuentaContable).Include(x => x.ProductoServicio).LoadAsync(ct);
            if (transaction is not null) await transaction.CommitAsync(ct);
            return Map(venta);
        }
        catch
        {
            if (transaction is not null) await transaction.RollbackAsync(ct);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    private static VentaDto Map(Venta x) => new(x.Id, x.Fecha, x.Cliente?.RazonSocial ?? string.Empty, $"{x.TipoComprobante} {x.Serie}-{x.Numero}", x.Subtotal, x.Igv, x.Total, x.FormaPago, x.AsientoContableId, x.Detalles.Select(d => new VentaDetalleDto($"{d.CuentaContable?.Codigo} - {d.CuentaContable?.Nombre}", d.Descripcion, d.Cantidad, d.PrecioUnitario, d.Total, d.ProductoServicioId, d.ProductoServicio?.Nombre)).ToList(), x.AsientoCostoVentaId, CorreccionesService.Estado(x.Correcciones), x.Correcciones.Sum(c => c.Subtotal), x.Correcciones.Sum(c => c.Igv));
}
