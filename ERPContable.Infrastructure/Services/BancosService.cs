using ERPContable.Application.Dtos;
using ERPContable.Application.Interfaces;
using ERPContable.Domain.Entities;
using ERPContable.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPContable.Infrastructure.Services;

public sealed class BancosService : IBancosService
{
    private readonly ERPDbContext _db;
    private readonly ICierreContableService _cierres;
    public BancosService(ERPDbContext db, ICierreContableService cierres) { _db = db; _cierres = cierres; }

    public async Task<IReadOnlyList<CuentaBancariaDto>> ObtenerCuentasAsync(int empresaId, CancellationToken ct = default)
        => await _db.CuentasBancarias.AsNoTracking().Include(x => x.CuentaContable).Where(x => x.EmpresaId == empresaId).OrderBy(x => x.Banco).Select(x => new CuentaBancariaDto(x.Id, x.Banco, x.NumeroCuenta, x.Moneda, $"{x.CuentaContable!.Codigo} - {x.CuentaContable.Nombre}", x.Activa)).ToListAsync(ct);

    public async Task<CuentaBancariaDto> CrearCuentaAsync(int empresaId, CuentaBancariaCreateDto request, CancellationToken ct = default)
    {
        if (!await _db.Empresas.AnyAsync(x => x.Id == empresaId, ct)) throw new ArgumentException("La empresa indicada no existe.");
        if (await _db.CuentasBancarias.AnyAsync(x => x.EmpresaId == empresaId && x.NumeroCuenta == request.NumeroCuenta, ct)) throw new ArgumentException("Ya existe esa cuenta bancaria.");
        var cuenta = await _db.CuentasContables.FirstOrDefaultAsync(x => x.Id == request.CuentaContableId && x.EmpresaId == empresaId && x.Activa, ct) ?? throw new ArgumentException("La cuenta contable no existe en la empresa o está inactiva.");
        if (!cuenta.Codigo.StartsWith("10", StringComparison.Ordinal)) throw new ArgumentException("La cuenta bancaria debe pertenecer al grupo 10.");
        var entity = CuentaBancaria.Crear(empresaId, request.Banco, request.NumeroCuenta, request.Moneda, request.CuentaContableId); _db.CuentasBancarias.Add(entity); await _db.SaveChangesAsync(ct);
        return new CuentaBancariaDto(entity.Id, entity.Banco, entity.NumeroCuenta, entity.Moneda, $"{cuenta.Codigo} - {cuenta.Nombre}", entity.Activa);
    }

    public async Task<IReadOnlyList<MovimientoBancoDto>> ObtenerMovimientosAsync(int empresaId, CancellationToken ct = default)
    {
        var items = await _db.MovimientosBanco.AsNoTracking().Include(x => x.CuentaBancaria).ThenInclude(x => x!.CuentaContable).Where(x => x.EmpresaId == empresaId).OrderByDescending(x => x.Fecha).ThenByDescending(x => x.Id).ToListAsync(ct);
        var ids = items.Select(x => x.CuentaContrapartidaId).Distinct().ToList(); var cuentas = await _db.CuentasContables.Where(x => x.EmpresaId == empresaId && ids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
        return items.Select(x => new MovimientoBancoDto(x.Id, x.Fecha, x.CuentaBancaria?.Banco ?? string.Empty, x.CuentaBancaria?.NumeroCuenta ?? string.Empty, x.Tipo, x.Descripcion, x.Monto, cuentas.TryGetValue(x.CuentaContrapartidaId, out var c) ? $"{c.Codigo} - {c.Nombre}" : string.Empty, x.AsientoContableId)).ToList();
    }

    public async Task<MovimientoBancoDto> CrearMovimientoAsync(int empresaId, MovimientoBancoCreateDto request, CancellationToken ct = default)
    {
        await _cierres.VerificarPeriodoAbiertoAsync(empresaId, request.Fecha, ct);
        var cuenta = await _db.CuentasBancarias.Include(x => x.CuentaContable).FirstOrDefaultAsync(x => x.Id == request.CuentaBancariaId && x.EmpresaId == empresaId && x.Activa, ct) ?? throw new ArgumentException("La cuenta bancaria no existe o está inactiva.");
        var contrapartida = await _db.CuentasContables.FirstOrDefaultAsync(x => x.Id == request.CuentaContrapartidaId && x.EmpresaId == empresaId && x.Activa, ct) ?? throw new ArgumentException("La cuenta de contrapartida no existe en la empresa o está inactiva.");
        var movimiento = MovimientoBanco.Crear(empresaId, cuenta.Id, request.Fecha, request.Tipo, request.Descripcion, request.Monto, contrapartida.Id); var ingreso = movimiento.Tipo == "INGRESO";
        var asiento = AsientoContable.Crear(empresaId, movimiento.Fecha, $"Banco {cuenta.Banco} - {movimiento.Descripcion}", new[] { (cuenta.CuentaContableId, ingreso ? movimiento.Monto : 0m, ingreso ? 0m : movimiento.Monto), (contrapartida.Id, ingreso ? 0m : movimiento.Monto, ingreso ? movimiento.Monto : 0m) });
        _db.MovimientosBanco.Add(movimiento); _db.AsientosContables.Add(asiento); await _db.SaveChangesAsync(ct); movimiento.AsignarAsiento(asiento.Id); await _db.SaveChangesAsync(ct);
        return new MovimientoBancoDto(movimiento.Id, movimiento.Fecha, cuenta.Banco, cuenta.NumeroCuenta, movimiento.Tipo, movimiento.Descripcion, movimiento.Monto, $"{contrapartida.Codigo} - {contrapartida.Nombre}", movimiento.AsientoContableId);
    }
}
