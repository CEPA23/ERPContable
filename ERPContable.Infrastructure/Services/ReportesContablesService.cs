using ERPContable.Application.Dtos;
using ERPContable.Application.Interfaces;
using ERPContable.Domain.Entities;
using ERPContable.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPContable.Infrastructure.Services;

public sealed class ReportesContablesService : IReportesContablesService
{
    private readonly ERPDbContext _db;

    public ReportesContablesService(ERPDbContext db) => _db = db;

    public async Task<LibroDiarioDto> ObtenerLibroDiarioAsync(int empresaId, DateTime desde, DateTime hasta, CancellationToken ct = default)
    {
        var periodo = NormalizarPeriodo(desde, hasta);
        var movimientos = await CargarMovimientosAsync(empresaId, periodo.Desde, periodo.Hasta, ct);
        var lineas = movimientos
            .OrderBy(x => x.Fecha).ThenBy(x => x.AsientoId).ThenBy(x => x.Codigo)
            .Select(x => new LibroDiarioLineaDto(x.Fecha, x.AsientoId, x.Glosa, x.Codigo, x.Nombre, x.Debe, x.Haber))
            .ToList();

        return new LibroDiarioDto(periodo.Desde, periodo.Hasta, lineas, lineas.Sum(x => x.Debe), lineas.Sum(x => x.Haber));
    }

    public async Task<IReadOnlyList<LibroMayorCuentaDto>> ObtenerLibroMayorAsync(int empresaId, DateTime desde, DateTime hasta, CancellationToken ct = default)
    {
        var periodo = NormalizarPeriodo(desde, hasta);
        var movimientos = await CargarMovimientosAsync(empresaId, periodo.Desde, periodo.Hasta, ct);

        return movimientos.GroupBy(x => new { x.CuentaId, x.Codigo, x.Nombre, x.Tipo })
            .OrderBy(x => x.Key.Codigo)
            .Select(group =>
            {
                var saldo = 0m;
                var detalles = group.OrderBy(x => x.Fecha).ThenBy(x => x.AsientoId).Select(x =>
                {
                    saldo += SaldoNormal(x.Tipo, x.Debe, x.Haber);
                    return new LibroMayorMovimientoDto(x.Fecha, x.AsientoId, x.Glosa, x.Debe, x.Haber, saldo);
                }).ToList();
                var debe = group.Sum(x => x.Debe);
                var haber = group.Sum(x => x.Haber);
                var saldoFinal = SaldoNormal(group.Key.Tipo, debe, haber);
                return new LibroMayorCuentaDto(group.Key.CuentaId, group.Key.Codigo, group.Key.Nombre, group.Key.Tipo,
                    debe, haber, Math.Max(saldoFinal, 0), Math.Max(-saldoFinal, 0), detalles);
            }).ToList();
    }

    public async Task<BalanceComprobacionDto> ObtenerBalanceComprobacionAsync(int empresaId, DateTime desde, DateTime hasta, CancellationToken ct = default)
    {
        var periodo = NormalizarPeriodo(desde, hasta);
        var movimientos = await CargarMovimientosAsync(empresaId, periodo.Desde, periodo.Hasta, ct);
        var lineas = movimientos.GroupBy(x => new { x.CuentaId, x.Codigo, x.Nombre, x.Tipo })
            .OrderBy(x => x.Key.Codigo).Select(x =>
            {
                var debe = x.Sum(y => y.Debe);
                var haber = x.Sum(y => y.Haber);
                var saldo = SaldoNormal(x.Key.Tipo, debe, haber);
                return new BalanceComprobacionLineaDto(x.Key.CuentaId, x.Key.Codigo, x.Key.Nombre, x.Key.Tipo,
                    debe, haber, Math.Max(saldo, 0), Math.Max(-saldo, 0));
            }).ToList();

        return new BalanceComprobacionDto(periodo.Desde, periodo.Hasta, lineas,
            lineas.Sum(x => x.Debe), lineas.Sum(x => x.Haber), lineas.Sum(x => x.SaldoDeudor), lineas.Sum(x => x.SaldoAcreedor));
    }

    public async Task<EstadoResultadosDto> ObtenerEstadoResultadosAsync(int empresaId, DateTime desde, DateTime hasta, CancellationToken ct = default)
    {
        var periodo = NormalizarPeriodo(desde, hasta);
        var movimientos = await CargarMovimientosAsync(empresaId, periodo.Desde, periodo.Hasta, ct);
        var ingresos = movimientos.Where(x => EsTipo(x.Tipo, "INGRESO")).Sum(x => x.Haber - x.Debe);
        var gastos = movimientos.Where(x => EsTipo(x.Tipo, "GASTO")).Sum(x => x.Debe - x.Haber);
        return new EstadoResultadosDto(periodo.Desde, periodo.Hasta, ingresos, gastos, ingresos - gastos);
    }

    public async Task<BalanceGeneralDto> ObtenerBalanceGeneralAsync(int empresaId, DateTime hasta, CancellationToken ct = default)
    {
        var fecha = hasta.Date;
        var movimientos = await CargarMovimientosAsync(empresaId, DateTime.MinValue.Date, fecha, ct);
        var activos = movimientos.Where(x => EsTipo(x.Tipo, "ACTIVO")).Sum(x => SaldoNormal(x.Tipo, x.Debe, x.Haber));
        var pasivos = movimientos.Where(x => EsTipo(x.Tipo, "PASIVO")).Sum(x => -SaldoNormal(x.Tipo, x.Debe, x.Haber));
        var patrimonio = movimientos.Where(x => EsTipo(x.Tipo, "PATRIMONIO")).Sum(x => -SaldoNormal(x.Tipo, x.Debe, x.Haber));
        var ingresos = movimientos.Where(x => EsTipo(x.Tipo, "INGRESO")).Sum(x => x.Haber - x.Debe);
        var gastos = movimientos.Where(x => EsTipo(x.Tipo, "GASTO")).Sum(x => x.Debe - x.Haber);
        var resultado = ingresos - gastos;
        return new BalanceGeneralDto(fecha, activos, pasivos, patrimonio, resultado, pasivos + patrimonio + resultado);
    }

    public async Task<FlujoCajaDto> ObtenerFlujoCajaAsync(int empresaId, DateTime desde, DateTime hasta, CancellationToken ct = default)
    {
        var periodo = NormalizarPeriodo(desde, hasta);
        var movimientos = await CargarMovimientosAsync(empresaId, periodo.Desde, periodo.Hasta, ct);
        var lineas = movimientos.Where(x => x.Codigo.StartsWith("10", StringComparison.Ordinal))
            .OrderBy(x => x.Fecha).ThenBy(x => x.AsientoId)
            .Select(x => new FlujoCajaMovimientoDto(x.Fecha, x.AsientoId, x.Glosa, x.Codigo, x.Nombre, x.Debe, x.Haber)).ToList();
        return new FlujoCajaDto(periodo.Desde, periodo.Hasta, lineas.Sum(x => x.Entrada), lineas.Sum(x => x.Salida),
            lineas.Sum(x => x.Entrada - x.Salida), lineas);
    }

    private async Task<List<Movimiento>> CargarMovimientosAsync(int empresaId, DateTime desde, DateTime hasta, CancellationToken ct)
    {
        if (!await _db.Empresas.AnyAsync(x => x.Id == empresaId, ct))
            throw new ArgumentException("La empresa indicada no existe.");

        var fin = hasta.Date.AddDays(1);
        var asientos = await _db.AsientosContables.AsNoTracking()
            .Include(x => x.Detalles).ThenInclude(x => x.CuentaContable)
            .Where(x => x.EmpresaId == empresaId && x.Fecha >= desde.Date && x.Fecha < fin)
            .ToListAsync(ct);

        return asientos.SelectMany(a => a.Detalles.Select(d => new Movimiento(
            a.Fecha, a.Id, a.Glosa, d.CuentaContableId, d.CuentaContable?.Codigo ?? string.Empty,
            d.CuentaContable?.Nombre ?? string.Empty, d.CuentaContable?.Tipo ?? string.Empty, d.Debe, d.Haber))).ToList();
    }

    private static (DateTime Desde, DateTime Hasta) NormalizarPeriodo(DateTime desde, DateTime hasta)
    {
        var inicio = desde.Date;
        var fin = hasta.Date;
        if (inicio > fin) throw new ArgumentException("La fecha desde no puede ser posterior a la fecha hasta.");
        return (inicio, fin);
    }

    private static decimal SaldoNormal(string tipo, decimal debe, decimal haber)
        => EsTipo(tipo, "ACTIVO") || EsTipo(tipo, "GASTO") ? debe - haber : haber - debe;

    private static bool EsTipo(string tipo, string esperado) => string.Equals(tipo, esperado, StringComparison.OrdinalIgnoreCase);

    private sealed record Movimiento(DateTime Fecha, int AsientoId, string Glosa, int CuentaId, string Codigo,
        string Nombre, string Tipo, decimal Debe, decimal Haber);
}
