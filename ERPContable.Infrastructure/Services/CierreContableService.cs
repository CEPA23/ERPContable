using ERPContable.Application.Dtos;
using ERPContable.Application.Interfaces;
using ERPContable.Domain.Entities;
using ERPContable.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPContable.Infrastructure.Services;

public sealed class CierreContableService : ICierreContableService
{
    private readonly ERPDbContext _db;
    public CierreContableService(ERPDbContext db) => _db = db;

    public async Task<EstadoCierresDto> ObtenerEstadoAsync(int empresaId, int ejercicio, CancellationToken cancellationToken = default)
    {
        var periodos = await _db.CierresPeriodos.AsNoTracking().Where(x => x.EmpresaId == empresaId && x.Ejercicio == ejercicio).OrderBy(x => x.Mes).ToListAsync(cancellationToken);
        var cierreEjercicio = await _db.CierresEjercicios.AsNoTracking().FirstOrDefaultAsync(x => x.EmpresaId == empresaId && x.Ejercicio == ejercicio, cancellationToken);
        var asientos = await _db.AsientosContables.CountAsync(x => x.EmpresaId == empresaId && x.Fecha.Year == ejercicio, cancellationToken);
        return new EstadoCierresDto(ejercicio, Enumerable.Range(1, 12).Select(mes => Map(periodos.FirstOrDefault(x => x.Mes == mes), ejercicio, mes)).ToList(), cierreEjercicio is null ? null : Map(cierreEjercicio), asientos);
    }

    public async Task<CierrePeriodoDto> CerrarPeriodoAsync(int empresaId, int ejercicio, int mes, string usuarioId, CerrarPeriodoDto request, CancellationToken cancellationToken = default)
    {
        ValidarEjercicioMes(ejercicio, mes);
        if (await EjercicioCerradoAsync(empresaId, ejercicio, cancellationToken)) throw new ArgumentException("El ejercicio está cerrado. Reábrelo antes de modificar un período.");
        var item = await _db.CierresPeriodos.FirstOrDefaultAsync(x => x.EmpresaId == empresaId && x.Ejercicio == ejercicio && x.Mes == mes, cancellationToken);
        if (item?.Cerrado == true) throw new ArgumentException("El período ya se encuentra cerrado.");
        if (item is null)
        {
            item = CierrePeriodo.Cerrar(empresaId, ejercicio, mes, usuarioId, request.Observacion);
            _db.CierresPeriodos.Add(item);
        }
        else item.Cerrar(usuarioId, request.Observacion);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(item);
    }

    public async Task<CierrePeriodoDto> ReabrirPeriodoAsync(int empresaId, int ejercicio, int mes, string usuarioId, CerrarPeriodoDto request, CancellationToken cancellationToken = default)
    {
        ValidarEjercicioMes(ejercicio, mes);
        if (await EjercicioCerradoAsync(empresaId, ejercicio, cancellationToken)) throw new ArgumentException("Primero debes reabrir el ejercicio.");
        var item = await _db.CierresPeriodos.FirstOrDefaultAsync(x => x.EmpresaId == empresaId && x.Ejercicio == ejercicio && x.Mes == mes, cancellationToken) ?? throw new ArgumentException("El período no tiene un cierre registrado.");
        if (!item.Cerrado) throw new ArgumentException("El período ya se encuentra abierto.");
        item.Reabrir(usuarioId, request.Observacion);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(item);
    }

    public async Task<CierreEjercicioDto> CerrarEjercicioAsync(int empresaId, int ejercicio, string usuarioId, CerrarEjercicioDto request, CancellationToken cancellationToken = default)
    {
        if (await EjercicioCerradoAsync(empresaId, ejercicio, cancellationToken)) throw new ArgumentException("El ejercicio ya se encuentra cerrado.");
        var cerrados = await _db.CierresPeriodos.CountAsync(x => x.EmpresaId == empresaId && x.Ejercicio == ejercicio && x.Cerrado, cancellationToken);
        if (cerrados != 12) throw new ArgumentException("Debes cerrar los doce períodos antes de cerrar el ejercicio.");
        if (await _db.AsientosContables.AnyAsync(x => x.EmpresaId == empresaId && x.Fecha.Year > ejercicio, cancellationToken))
            throw new ArgumentException("No puedes cerrar este ejercicio porque ya existen asientos en ejercicios posteriores.");

        var configuracion = await _db.ConfiguracionesContablesEmpresas.FirstOrDefaultAsync(x => x.EmpresaId == empresaId, cancellationToken);
        if (configuracion is null) throw new ArgumentException("Configura la cuenta de resultados acumulados antes de cerrar el ejercicio.");
        var cuentaResultado = await _db.CuentasContables.FirstOrDefaultAsync(x => x.Id == configuracion.CuentaResultadoAcumuladoId && x.EmpresaId == empresaId, cancellationToken);
        if (cuentaResultado is null || !cuentaResultado.Activa || !string.Equals(cuentaResultado.Tipo, "PATRIMONIO", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("La cuenta configurada para resultados acumulados debe estar activa y ser de patrimonio.");

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        var inicioEjercicio = new DateTime(ejercicio, 1, 1);
        var finEjercicio = new DateTime(ejercicio, 12, 31);
        var saldosResultado = await ObtenerSaldosAsync(empresaId, inicioEjercicio, finEjercicio, ["INGRESO", "GASTO"], cancellationToken);
        var detallesCierre = CrearDetallesCierre(saldosResultado, cuentaResultado.Id);
        AsientoContable? asientoCierre = null;
        if (detallesCierre.Count >= 2)
        {
            asientoCierre = AsientoContable.Crear(empresaId, finEjercicio, $"Cierre automático del ejercicio {ejercicio}", detallesCierre);
            _db.AsientosContables.Add(asientoCierre);
        }

        // La apertura toma los saldos del balance y agrega el resultado que el cierre
        // transfirió a patrimonio. Los resultados (6 y 7) no se arrastran al nuevo año.
        var saldosApertura = await ObtenerSaldosAsync(empresaId, DateTime.MinValue, finEjercicio, ["ACTIVO", "PASIVO", "PATRIMONIO"], cancellationToken);
        var resultadoEjercicio = saldosResultado.Sum(x => x.Saldo);
        if (resultadoEjercicio != 0) AgregarSaldo(saldosApertura, cuentaResultado.Id, "PATRIMONIO", -resultadoEjercicio);
        var detallesApertura = CrearDetallesApertura(saldosApertura);
        if (detallesApertura.Count > 0 && detallesApertura.Sum(x => x.debe) != detallesApertura.Sum(x => x.haber))
            throw new ArgumentException("El asiento de apertura no cuadra. Revisa los saldos y asientos anteriores antes de cerrar.");

        AsientoContable? asientoApertura = null;
        if (detallesApertura.Count >= 2)
        {
            asientoApertura = AsientoContable.Crear(empresaId, new DateTime(ejercicio + 1, 1, 1), $"Apertura automática del ejercicio {ejercicio + 1}", detallesApertura);
            _db.AsientosContables.Add(asientoApertura);
        }

        var item = await _db.CierresEjercicios.FirstOrDefaultAsync(x => x.EmpresaId == empresaId && x.Ejercicio == ejercicio, cancellationToken);
        if (item is null)
        {
            item = CierreEjercicio.Cerrar(empresaId, ejercicio, usuarioId, request.Observacion);
            _db.CierresEjercicios.Add(item);
        }
        else item.Cerrar(usuarioId, request.Observacion);

        await _db.SaveChangesAsync(cancellationToken);
        item.RegistrarAsientosAutomaticos(asientoCierre?.Id, asientoApertura?.Id);
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Map(item);
    }

    public async Task<CierreEjercicioDto> ReabrirEjercicioAsync(int empresaId, int ejercicio, string usuarioId, CerrarEjercicioDto request, CancellationToken cancellationToken = default)
    {
        var item = await _db.CierresEjercicios.FirstOrDefaultAsync(x => x.EmpresaId == empresaId && x.Ejercicio == ejercicio, cancellationToken) ?? throw new ArgumentException("El ejercicio no tiene un cierre registrado.");
        if (!item.Cerrado) throw new ArgumentException("El ejercicio ya se encuentra abierto.");

        var idsAutomaticos = new[] { item.AsientoCierreId, item.AsientoAperturaId }.Where(x => x.HasValue).Select(x => x!.Value).ToArray();
        if (await _db.AsientosContables.AnyAsync(x => x.EmpresaId == empresaId && x.Fecha.Year > ejercicio && !idsAutomaticos.Contains(x.Id), cancellationToken))
            throw new ArgumentException("No puedes reabrir el ejercicio porque existen asientos posteriores. Anúlalos antes de reabrir.");

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        if (idsAutomaticos.Length > 0)
        {
            var asientos = await _db.AsientosContables.Where(x => idsAutomaticos.Contains(x.Id)).ToListAsync(cancellationToken);
            _db.AsientosContables.RemoveRange(asientos);
        }
        item.Reabrir(usuarioId, request.Observacion);
        item.RegistrarAsientosAutomaticos(null, null);
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Map(item);
    }

    public async Task VerificarPeriodoAbiertoAsync(int empresaId, DateTime fecha, CancellationToken cancellationToken = default)
    {
        if (await EjercicioCerradoAsync(empresaId, fecha.Year, cancellationToken)) throw new ArgumentException($"El ejercicio {fecha.Year} está cerrado.");
        if (await _db.CierresPeriodos.AnyAsync(x => x.EmpresaId == empresaId && x.Ejercicio == fecha.Year && x.Mes == fecha.Month && x.Cerrado, cancellationToken)) throw new ArgumentException($"El período {fecha:MM/yyyy} está cerrado.");
    }

    private async Task<List<SaldoCuenta>> ObtenerSaldosAsync(int empresaId, DateTime desde, DateTime hasta, IReadOnlyCollection<string> tipos, CancellationToken cancellationToken)
        => await _db.AsientosContables.AsNoTracking()
            .Where(x => x.EmpresaId == empresaId && x.Fecha >= desde && x.Fecha <= hasta)
            .SelectMany(x => x.Detalles.Select(d => new { d.CuentaContableId, d.Debe, d.Haber, Tipo = d.CuentaContable!.Tipo }))
            .Where(x => tipos.Contains(x.Tipo))
            .GroupBy(x => new { x.CuentaContableId, x.Tipo })
            .Select(x => new SaldoCuenta(x.Key.CuentaContableId, x.Key.Tipo, x.Sum(y => y.Debe) - x.Sum(y => y.Haber)))
            .Where(x => x.Saldo != 0)
            .ToListAsync(cancellationToken);

    private static List<(int cuentaId, decimal debe, decimal haber)> CrearDetallesCierre(IEnumerable<SaldoCuenta> saldos, int cuentaResultadoId)
    {
        var detalles = saldos.Select(x => x.Saldo > 0 ? (x.CuentaId, debe: 0m, haber: x.Saldo) : (x.CuentaId, debe: -x.Saldo, haber: 0m)).ToList();
        var diferencia = detalles.Sum(x => x.debe) - detalles.Sum(x => x.haber);
        if (diferencia > 0) detalles.Add((cuentaResultadoId, 0m, diferencia));
        else if (diferencia < 0) detalles.Add((cuentaResultadoId, -diferencia, 0m));
        return detalles;
    }

    private static void AgregarSaldo(List<SaldoCuenta> saldos, int cuentaId, string tipo, decimal saldo)
    {
        var indice = saldos.FindIndex(x => x.CuentaId == cuentaId);
        if (indice >= 0) saldos[indice] = saldos[indice] with { Saldo = saldos[indice].Saldo + saldo };
        else saldos.Add(new SaldoCuenta(cuentaId, tipo, saldo));
    }

    private static List<(int cuentaId, decimal debe, decimal haber)> CrearDetallesApertura(IEnumerable<SaldoCuenta> saldos)
        => saldos.Where(x => x.Saldo != 0).Select(x => x.Saldo > 0 ? (x.CuentaId, debe: x.Saldo, haber: 0m) : (x.CuentaId, debe: 0m, haber: -x.Saldo)).ToList();

    private Task<bool> EjercicioCerradoAsync(int empresaId, int ejercicio, CancellationToken cancellationToken)
        => _db.CierresEjercicios.AnyAsync(x => x.EmpresaId == empresaId && x.Ejercicio == ejercicio && x.Cerrado, cancellationToken);

    private static void ValidarEjercicioMes(int ejercicio, int mes)
    {
        if (ejercicio is < 2000 or > 2200 || mes is < 1 or > 12) throw new ArgumentException("Ejercicio o mes no válido.");
    }

    private static CierrePeriodoDto Map(CierrePeriodo? item, int ejercicio, int mes) => item is null ? new CierrePeriodoDto(0, ejercicio, mes, false, null, null, null) : Map(item);
    private static CierrePeriodoDto Map(CierrePeriodo item) => new(item.Id, item.Ejercicio, item.Mes, item.Cerrado, item.CerradoEnUtc, item.ReabiertoEnUtc, item.Observacion);
    private static CierreEjercicioDto Map(CierreEjercicio item) => new(item.Id, item.Ejercicio, item.Cerrado, item.CerradoEnUtc, item.ReabiertoEnUtc, item.Observacion, item.AsientoCierreId, item.AsientoAperturaId);
    private sealed record SaldoCuenta(int CuentaId, string Tipo, decimal Saldo);
}
