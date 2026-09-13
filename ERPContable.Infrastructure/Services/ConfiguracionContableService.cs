using ERPContable.Application.Dtos;
using ERPContable.Application.Interfaces;
using ERPContable.Domain.Entities;
using ERPContable.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPContable.Infrastructure.Services;

public sealed class ConfiguracionContableService : IConfiguracionContableService
{
    private readonly ERPDbContext _db;

    public ConfiguracionContableService(ERPDbContext db) => _db = db;

    public async Task<ConfiguracionContableDto> ObtenerAsync(int empresaId, CancellationToken cancellationToken = default)
    {
        var config = await _db.ConfiguracionesContablesEmpresas.AsNoTracking().FirstOrDefaultAsync(x => x.EmpresaId == empresaId, cancellationToken);
        if (config is null) return Empty(empresaId);

        var ids = new[] { config.CuentaResultadoAcumuladoId, config.CuentaInventarioId, config.CuentaCostoVentasId }.Where(x => x.HasValue).Select(x => x!.Value).ToArray();
        var cuentas = await _db.CuentasContables.AsNoTracking().Where(x => x.EmpresaId == empresaId && ids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, cancellationToken);
        return new ConfiguracionContableDto(empresaId,
            config.CuentaResultadoAcumuladoId, Format(cuentas, config.CuentaResultadoAcumuladoId),
            config.CuentaInventarioId, Format(cuentas, config.CuentaInventarioId),
            config.CuentaCostoVentasId, Format(cuentas, config.CuentaCostoVentasId));
    }

    public async Task<ConfiguracionContableDto> ActualizarAsync(int empresaId, ActualizarConfiguracionContableDto request, CancellationToken cancellationToken = default)
    {
        if (!await _db.Empresas.AnyAsync(x => x.Id == empresaId && x.Activa, cancellationToken)) throw new ArgumentException("La empresa indicada no existe o está inactiva.");
        var ids = new[] { request.CuentaResultadoAcumuladoId, request.CuentaInventarioId, request.CuentaCostoVentasId }.Distinct().ToArray();
        if (ids.Length != 3) throw new ArgumentException("Selecciona tres cuentas contables diferentes.");
        var cuentas = await _db.CuentasContables.Where(x => x.EmpresaId == empresaId && ids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, cancellationToken);
        if (cuentas.Count != 3) throw new ArgumentException("Una o más cuentas no pertenecen a la empresa.");
        Validate(cuentas[request.CuentaResultadoAcumuladoId], "resultados acumulados", "PATRIMONIO");
        Validate(cuentas[request.CuentaInventarioId], "inventario", "ACTIVO");
        Validate(cuentas[request.CuentaCostoVentasId], "costo de ventas", "GASTO");

        var config = await _db.ConfiguracionesContablesEmpresas.FirstOrDefaultAsync(x => x.EmpresaId == empresaId, cancellationToken);
        if (config is null) _db.ConfiguracionesContablesEmpresas.Add(ConfiguracionContableEmpresa.Crear(empresaId, request.CuentaResultadoAcumuladoId, request.CuentaInventarioId, request.CuentaCostoVentasId));
        else config.Actualizar(request.CuentaResultadoAcumuladoId, request.CuentaInventarioId, request.CuentaCostoVentasId);
        await _db.SaveChangesAsync(cancellationToken);
        return new ConfiguracionContableDto(empresaId,
            request.CuentaResultadoAcumuladoId, Format(cuentas, request.CuentaResultadoAcumuladoId),
            request.CuentaInventarioId, Format(cuentas, request.CuentaInventarioId),
            request.CuentaCostoVentasId, Format(cuentas, request.CuentaCostoVentasId));
    }

    private static void Validate(CuentaContable account, string purpose, string expectedType)
    {
        if (!account.Activa || !string.Equals(account.Tipo, expectedType, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException($"Selecciona una cuenta activa de tipo {expectedType} para {purpose}.");
    }

    private static string? Format(IReadOnlyDictionary<int, CuentaContable> accounts, int? id)
        => id.HasValue && accounts.TryGetValue(id.Value, out var account) ? $"{account.Codigo} - {account.Nombre}" : null;

    private static ConfiguracionContableDto Empty(int empresaId) => new(empresaId, null, null, null, null, null, null);
}
