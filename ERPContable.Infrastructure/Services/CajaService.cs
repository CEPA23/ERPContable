using ERPContable.Application.Dtos;
using ERPContable.Application.Interfaces;
using ERPContable.Domain.Entities;
using ERPContable.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPContable.Infrastructure.Services;

public sealed class CajaService : ICajaService
{
    private readonly ERPDbContext _db;
    private readonly ICierreContableService _cierres;
    public CajaService(ERPDbContext db, ICierreContableService cierres) { _db = db; _cierres = cierres; }

    public async Task<IReadOnlyList<MovimientoCajaDto>> ObtenerMovimientosAsync(int empresaId, CancellationToken ct = default)
    {
        var items = await _db.MovimientosCaja.AsNoTracking()
            .Where(x => x.EmpresaId == empresaId)
            .OrderByDescending(x => x.Fecha).ThenByDescending(x => x.Id)
            .ToListAsync(ct);
        var cuentaIds = items.SelectMany(x => new[] { x.CuentaCajaId, x.CuentaContrapartidaId }).Distinct().ToList();
        var cuentas = await _db.CuentasContables.Where(x => x.EmpresaId == empresaId && cuentaIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
        return items.Select(x => Map(x, cuentas)).ToList();
    }

    public async Task<MovimientoCajaDto> CrearMovimientoAsync(int empresaId, MovimientoCajaCreateDto request, CancellationToken ct = default)
    {
        await _cierres.VerificarPeriodoAbiertoAsync(empresaId, request.Fecha, ct);
        if (!await _db.Empresas.AnyAsync(x => x.Id == empresaId, ct)) throw new ArgumentException("La empresa indicada no existe.");
        var cuentas = await _db.CuentasContables.Where(x => x.EmpresaId == empresaId && (x.Id == request.CuentaCajaId || x.Id == request.CuentaContrapartidaId)).ToListAsync(ct);
        if (cuentas.Count != 2 || cuentas.Any(x => !x.Activa)) throw new ArgumentException("Las cuentas indicadas no existen o están inactivas.");
        if (!cuentas.Single(x => x.Id == request.CuentaCajaId).Codigo.StartsWith("10", StringComparison.Ordinal)) throw new ArgumentException("La cuenta de caja debe comenzar con el código 10.");
        var movimiento = MovimientoCaja.Crear(empresaId, request.Fecha, request.Tipo, request.Descripcion, request.Monto, request.CuentaCajaId, request.CuentaContrapartidaId);
        var ingreso = string.Equals(movimiento.Tipo, "INGRESO", StringComparison.OrdinalIgnoreCase);
        var asiento = AsientoContable.Crear(empresaId, movimiento.Fecha, $"Caja - {movimiento.Descripcion}", new[]
        {
            (movimiento.CuentaCajaId, ingreso ? movimiento.Monto : 0m, ingreso ? 0m : movimiento.Monto),
            (movimiento.CuentaContrapartidaId, ingreso ? 0m : movimiento.Monto, ingreso ? movimiento.Monto : 0m)
        });
        _db.MovimientosCaja.Add(movimiento); _db.AsientosContables.Add(asiento);
        await _db.SaveChangesAsync(ct); movimiento.AsignarAsiento(asiento.Id); await _db.SaveChangesAsync(ct);
        var map = cuentas.ToDictionary(x => x.Id);
        return Map(movimiento, map);
    }

    private static MovimientoCajaDto Map(MovimientoCaja x, IReadOnlyDictionary<int, CuentaContable> cuentas)
        => new(x.Id, x.Fecha, x.Tipo, x.Descripcion, x.Monto, Cuenta(cuentas, x.CuentaCajaId), Cuenta(cuentas, x.CuentaContrapartidaId), x.AsientoContableId);
    private static string Cuenta(IReadOnlyDictionary<int, CuentaContable> cuentas, int id) => cuentas.TryGetValue(id, out var cuenta) ? $"{cuenta.Codigo} - {cuenta.Nombre}" : string.Empty;
}
