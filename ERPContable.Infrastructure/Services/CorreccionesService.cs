using ERPContable.Application.Dtos;
using ERPContable.Application.Interfaces;
using ERPContable.Domain.Entities;
using ERPContable.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPContable.Infrastructure.Services;

public sealed class CorreccionesService(ERPDbContext db, ICierreContableService cierres)
{
    private sealed record Linea(int Id, int CuentaId, int? ProductoId, string Descripcion, decimal Cantidad, decimal Precio, decimal Total, decimal? CostoUnitario, decimal? Costo);
    private sealed record Documento(int Id, DateTime Fecha, string Referencia, decimal Subtotal, decimal Igv, int Contrapartida, int CuentaIgv, int? AsientoId, int? CostoAsientoId, List<Linea> Lineas, List<CorreccionDocumento> Historial);

    public async Task<CorreccionesDocumentoDto> ObtenerAsync(int empresaId, bool venta, int id, CancellationToken ct = default)
        => Map(await Cargar(empresaId, venta, id, ct));

    public async Task<CorreccionesDocumentoDto> RegistrarAsync(int empresaId, bool venta, int id, CorreccionRequest request, string usuarioId, string nombre, CancellationToken ct = default)
    {
        if (request.SolicitudId == Guid.Empty) throw new ArgumentException("Falta el identificador de la solicitud.");
        if (request.Tipo is not ("ANULACION" or "DEVOLUCION")) throw new ArgumentException("Selecciona anulación o devolución.");
        if (string.IsNullOrWhiteSpace(request.Motivo) || request.Motivo.Trim().Length > 500) throw new ArgumentException("Indica un motivo de hasta 500 caracteres.");
        if (string.IsNullOrWhiteSpace(usuarioId)) throw new UnauthorizedAccessException();
        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct) : null;
        var doc = await Cargar(empresaId, venta, id, ct);
        var anterior = await db.CorreccionesDocumentos.AsNoTracking().FirstOrDefaultAsync(x => x.EmpresaId == empresaId && x.SolicitudId == request.SolicitudId, ct);
        if (anterior is not null)
        {
            if (anterior.VentaId != (venta ? id : null) || anterior.CompraId != (venta ? null : id))
                throw new ArgumentException("Esta solicitud ya pertenece a otro documento.");
            return Map(doc); // Safe retry after a lost response; never apply twice.
        }
        if (doc.Historial.Any(x => x.Completa)) throw new ArgumentException("El documento ya está anulado o devuelto totalmente.");
        if (request.Tipo == "ANULACION" && doc.Historial.Count > 0) throw new ArgumentException("El documento tiene devoluciones. Devuelve las cantidades restantes para completarlo.");
        if (request.Fecha.Date < doc.Fecha.Date) throw new ArgumentException("La corrección no puede ser anterior al documento original.");
        await cierres.VerificarPeriodoAbiertoAsync(empresaId, request.Fecha, ct);
        // A return may be posted in an open period; cancellation requires the original period open too.
        if (request.Tipo == "ANULACION") await cierres.VerificarPeriodoAbiertoAsync(empresaId, doc.Fecha, ct);
        var pendientes = Map(doc).Detalles.ToDictionary(x => x.Id);
        var seleccion = request.Tipo == "ANULACION"
            ? doc.Lineas.Select(x => new CorreccionLineaRequest(x.Id, x.Cantidad)).ToArray()
            : request.Detalles?.ToArray() ?? [];
        if (seleccion.Length == 0 || seleccion.Select(x => x.DetalleId).Distinct().Count() != seleccion.Length)
            throw new ArgumentException("Selecciona al menos una línea sin repetirla.");
        foreach (var item in seleccion)
            if (!pendientes.TryGetValue(item.DetalleId, out var linea) || item.Cantidad <= 0 || item.Cantidad != decimal.Round(item.Cantidad, 4) || item.Cantidad > linea.Disponible)
                throw new ArgumentException("Una cantidad no es válida o supera lo pendiente de devolver.");

        var asientoOriginal = await db.AsientosContables.AsNoTracking().Include(x => x.Detalles).FirstOrDefaultAsync(x => x.Id == doc.AsientoId && x.EmpresaId == empresaId, ct)
            ?? throw new ArgumentException("El documento no tiene un asiento original para revertir.");
        if (asientoOriginal.Detalles.Sum(x => x.Debe) != doc.Subtotal + doc.Igv)
            throw new ArgumentException("El asiento original no coincide con el documento. Revisa su contabilización.");
        AsientoContable? costoOriginal = null;
        if (venta && doc.Lineas.Any(x => x.ProductoId.HasValue))
        {
            costoOriginal = await db.AsientosContables.AsNoTracking().Include(x => x.Detalles).FirstOrDefaultAsync(x => x.Id == doc.CostoAsientoId && x.EmpresaId == empresaId, ct)
                ?? throw new ArgumentException("Esta venta antigua no tiene asiento de costo de ventas. Revisa su contabilización antes de devolverla.");
            if (costoOriginal.Detalles.Count != 2 || costoOriginal.Detalles.Count(x => x.Debe > 0) != 1 || costoOriginal.Detalles.Count(x => x.Haber > 0) != 1)
                throw new ArgumentException("El asiento original de costo requiere revisión.");
            doc = await RecuperarCostos(empresaId, doc, costoOriginal, ct);
        }

        var correccion = new CorreccionDocumento
        {
            EmpresaId = empresaId, CompraId = venta ? null : id, VentaId = venta ? id : null,
            Tipo = request.Tipo, Motivo = request.Motivo.Trim(), Fecha = DateTime.SpecifyKind(request.Fecha.Date, DateTimeKind.Unspecified),
            UsuarioId = usuarioId, UsuarioNombre = nombre, SolicitudId = request.SolicitudId,
            Completa = pendientes.Values.All(x => seleccion.Where(s => s.DetalleId == x.Id).Sum(s => s.Cantidad) == x.Disponible)
        };
        var lineasAsiento = new List<(int cuentaId, decimal debe, decimal haber)>();
        var stocks = new Dictionary<int, InventarioProducto>();
        var movimientos = new List<MovimientoInventario>();
        foreach (var item in seleccion)
        {
            var linea = doc.Lineas.Single(x => x.Id == item.DetalleId);
            var prev = doc.Historial.SelectMany(x => x.Detalles).Where(x => (venta ? x.VentaDetalleId : x.CompraDetalleId) == item.DetalleId).ToList();
            var cantidadAcumulada = prev.Sum(x => x.Cantidad) + item.Cantidad;
            var baseDevuelta = decimal.Round(linea.Total * cantidadAcumulada / linea.Cantidad, 2) - prev.Sum(x => x.Subtotal);
            var costo = linea.ProductoId.HasValue
                ? decimal.Round((venta ? linea.Costo!.Value : linea.Total) * cantidadAcumulada / linea.Cantidad, 2) - prev.Sum(x => x.CostoInventario)
                : 0;
            var detalle = new CorreccionDetalle { CompraDetalleId = venta ? null : linea.Id, VentaDetalleId = venta ? linea.Id : null, Cantidad = item.Cantidad, Subtotal = baseDevuelta, CostoInventario = costo };
            correccion.Detalles.Add(detalle);
            correccion.Subtotal += baseDevuelta;
            if (baseDevuelta > 0) lineasAsiento.Add((linea.CuentaId, venta ? baseDevuelta : 0, venta ? 0 : baseDevuelta));
            if (linea.ProductoId is int productoId)
            {
                if (!stocks.TryGetValue(productoId, out var stock))
                {
                    stock = await db.InventarioProductos.SingleOrDefaultAsync(x => x.EmpresaId == empresaId && x.ProductoServicioId == productoId, ct)
                        ?? throw new ArgumentException("No se encuentra el inventario del producto original.");
                    var fechaUtc = DateTime.SpecifyKind(request.Fecha.Date.AddDays(1), DateTimeKind.Utc);
                    if (await db.MovimientosInventario.AnyAsync(x => x.EmpresaId == empresaId && x.ProductoServicioId == productoId && x.FechaUtc >= fechaUtc, ct))
                        throw new ArgumentException("La fecha debe ser igual o posterior al último movimiento de los productos seleccionados.");
                    stocks.Add(productoId, stock);
                }
                var antes = stock.StockActual;
                var costoUnitario = venta ? linea.CostoUnitario!.Value : linea.Precio;
                stock.RevertirInventario(venta ? item.Cantidad : -item.Cantidad, (venta ? 1 : -1) * item.Cantidad * costoUnitario);
                var movimiento = MovimientoInventario.Crear(empresaId, productoId, venta ? "ENTRADA" : "SALIDA", item.Cantidad, costoUnitario, antes, stock.StockActual,
                    $"{request.Tipo} {(venta ? "VENTA" : "COMPRA")} #{id}", $"Corrección: {doc.Referencia}", request.Fecha);
                detalle.MovimientoInventario = movimiento;
                movimientos.Add(movimiento);
            }
        }
        var baseAnterior = doc.Historial.Sum(x => x.Subtotal);
        correccion.Igv = decimal.Round(doc.Igv * (baseAnterior + correccion.Subtotal) / doc.Subtotal, 2) - doc.Historial.Sum(x => x.Igv);
        if (correccion.Igv > 0) lineasAsiento.Add((doc.CuentaIgv, venta ? correccion.Igv : 0, venta ? 0 : correccion.Igv));
        var total = correccion.Subtotal + correccion.Igv;
        if (total > 0)
        {
            lineasAsiento.Add((doc.Contrapartida, venta ? 0 : total, venta ? total : 0));
            correccion.AsientoContable = AsientoContable.Crear(empresaId, correccion.Fecha, $"{request.Tipo} {(venta ? "venta" : "compra")} #{id} - {doc.Referencia}", lineasAsiento);
        }
        var totalCosto = correccion.Detalles.Sum(x => x.CostoInventario);
        if (venta && totalCosto > 0 && costoOriginal is not null)
        {
            var cuentaCosto = costoOriginal.Detalles.Single(x => x.Debe > 0).CuentaContableId;
            var cuentaInventario = costoOriginal.Detalles.Single(x => x.Haber > 0).CuentaContableId;
            correccion.AsientoCosto = AsientoContable.Crear(empresaId, correccion.Fecha, $"Reversión costo venta #{id} - {doc.Referencia}", [(cuentaInventario, totalCosto, 0), (cuentaCosto, 0, totalCosto)]);
        }
        db.CorreccionesDocumentos.Add(correccion);
        db.Auditoria.Add(new AuditLog { EmpresaId = empresaId, UsuarioId = usuarioId, Accion = request.Tipo, Entidad = venta ? "Venta" : "Compra", Detalles = System.Text.Json.JsonSerializer.Serialize(new { DocumentoId = id, request.SolicitudId, correccion.Motivo, Total = total }) });
        await db.SaveChangesAsync(ct);
        if (transaction is not null) await transaction.CommitAsync(ct);
        return await ObtenerAsync(empresaId, venta, id, ct);
    }

    private async Task<Documento> Cargar(int empresaId, bool venta, int id, CancellationToken ct)
    {
        if (!await db.Empresas.AnyAsync(x => x.Id == empresaId && x.Activa, ct)) throw new ArgumentException("La empresa no existe o está inactiva.");
        if (venta)
        {
            var x = await db.Ventas.AsNoTracking().Include(x => x.Detalles).Include(x => x.Correcciones).ThenInclude(x => x.Detalles).SingleOrDefaultAsync(x => x.Id == id && x.EmpresaId == empresaId, ct)
                ?? throw new ArgumentException("La venta no pertenece a la empresa.");
            return new(x.Id, x.Fecha, $"{x.TipoComprobante} {x.Serie}-{x.Numero}", x.Subtotal, x.Igv, x.CuentaContrapartidaId, x.CuentaIgvId, x.AsientoContableId, x.AsientoCostoVentaId,
                x.Detalles.OrderBy(d => d.Id).Select(d => new Linea(d.Id, d.CuentaContableId, d.ProductoServicioId, d.Descripcion, d.Cantidad, d.PrecioUnitario, d.Total, d.CostoUnitarioInventario, d.CostoInventario)).ToList(), x.Correcciones);
        }
        var c = await db.Compras.AsNoTracking().Include(x => x.Detalles).Include(x => x.Correcciones).ThenInclude(x => x.Detalles).SingleOrDefaultAsync(x => x.Id == id && x.EmpresaId == empresaId, ct)
            ?? throw new ArgumentException("La compra no pertenece a la empresa.");
        return new(c.Id, c.Fecha, $"{c.TipoComprobante} {c.Serie}-{c.Numero}", c.Subtotal, c.Igv, c.CuentaContrapartidaId, c.CuentaIgvId, c.AsientoContableId, null,
            c.Detalles.OrderBy(d => d.Id).Select(d => new Linea(d.Id, d.CuentaContableId, d.ProductoServicioId, d.Descripcion, d.Cantidad, d.PrecioUnitario, d.Total, d.PrecioUnitario, d.Total)).ToList(), c.Correcciones);
    }

    private async Task<Documento> RecuperarCostos(int empresaId, Documento doc, AsientoContable asiento, CancellationToken ct)
    {
        if (doc.Lineas.Any(x => x.ProductoId.HasValue && (!x.Costo.HasValue || !x.CostoUnitario.HasValue)))
        {
            // Legacy documents had only a textual movement reference. Accept it only when unambiguous.
            var referencias = await db.Ventas.AsNoTracking().Where(x => x.EmpresaId == empresaId).Select(x => new { x.TipoComprobante, x.Serie, x.Numero }).ToListAsync(ct);
            if (referencias.Count(x => $"{x.TipoComprobante} {x.Serie}-{x.Numero}" == doc.Referencia) != 1)
                throw new ArgumentException("La referencia de esta venta antigua es ambigua. Revisa sus costos antes de devolverla.");
            var movimientos = await db.MovimientosInventario.AsNoTracking().Where(x => x.EmpresaId == empresaId && x.Tipo == "SALIDA" && x.Referencia == doc.Referencia && x.Observacion == "Salida automática por venta").ToListAsync(ct);
            var costos = new Dictionary<int, decimal>();
            foreach (var grupo in doc.Lineas.Where(x => x.ProductoId.HasValue).GroupBy(x => x.ProductoId!.Value))
            {
                var matches = movimientos.Where(x => x.ProductoServicioId == grupo.Key).ToArray();
                if (matches.Length != 1 || matches[0].Cantidad != grupo.Sum(x => x.Cantidad))
                    throw new ArgumentException("No se puede identificar el costo original de esta venta antigua. Revisa su kardex.");
                costos[grupo.Key] = matches[0].CostoUnitario;
            }
            decimal acumulado = 0;
            doc = doc with { Lineas = doc.Lineas.Select(x =>
            {
                if (x.ProductoId is not int p) return x;
                var previo = decimal.Round(acumulado, 2);
                acumulado += x.Cantidad * costos[p];
                return x with { CostoUnitario = costos[p], Costo = decimal.Round(acumulado, 2) - previo };
            }).ToList() };
        }
        if (doc.Lineas.Where(x => x.ProductoId.HasValue).Sum(x => x.Costo ?? 0) != asiento.Detalles.Sum(x => x.Debe))
            throw new ArgumentException("Los costos del documento no coinciden con su asiento original.");
        return doc;
    }

    public static string Estado(IEnumerable<CorreccionDocumento> historial)
        => historial.Any(x => x.Tipo == "ANULACION") ? "ANULADO" : historial.Any(x => x.Completa) ? "DEVUELTO" : historial.Any() ? "DEVOLUCION_PARCIAL" : "VIGENTE";

    private static CorreccionesDocumentoDto Map(Documento doc)
        => new(doc.Id, doc.Referencia, doc.Fecha, Estado(doc.Historial), doc.Subtotal + doc.Igv, doc.Historial.Sum(x => x.Subtotal + x.Igv),
            doc.Lineas.Select(x =>
            {
                var cantidad = doc.Historial.SelectMany(c => c.Detalles).Where(d => (d.VentaDetalleId ?? d.CompraDetalleId) == x.Id).Sum(d => d.Cantidad);
                return new CorreccionLineaDisponible(x.Id, x.Descripcion, x.Cantidad, cantidad, x.Cantidad - cantidad, x.Precio);
            }).ToList(),
            doc.Historial.OrderByDescending(x => x.Id).Select(x => new CorreccionHistorialDto(x.Id, x.Tipo, x.Fecha, x.Motivo, x.UsuarioNombre, x.CreadoEnUtc, x.Subtotal, x.Igv, x.AsientoContableId, x.AsientoCostoId,
                x.Detalles.Select(d => new CorreccionLineaHistorialDto((d.VentaDetalleId ?? d.CompraDetalleId)!.Value, d.Cantidad, d.Subtotal, d.CostoInventario, d.MovimientoInventarioId)).ToList())).ToList());
}
