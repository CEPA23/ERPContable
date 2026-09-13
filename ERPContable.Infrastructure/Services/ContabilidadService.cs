using ERPContable.Application.Dtos;
using ERPContable.Application.Interfaces;
using ERPContable.Domain.Entities;
using ERPContable.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPContable.Infrastructure.Services;

public sealed class ContabilidadService : IContabilidadService
{
    private readonly ERPDbContext _db;
    private readonly ICierreContableService _cierres;
    public ContabilidadService(ERPDbContext db, ICierreContableService cierres) { _db = db; _cierres = cierres; }

    public async Task<IReadOnlyList<CuentaContableDto>> GetCuentasAsync(int empresaId, CancellationToken ct = default)
        => await _db.CuentasContables.AsNoTracking().Where(x => x.EmpresaId == empresaId).OrderBy(x => x.Codigo)
            .Select(x => new CuentaContableDto(x.Id, x.Codigo, x.Nombre, x.Tipo, x.Activa)).ToListAsync(ct);

    public async Task InicializarPlanContableAsync(int empresaId, CancellationToken ct = default)
    {
        var plantillas = await _db.CuentasContables.AsNoTracking().Where(x => x.EmpresaId == null).OrderBy(x => x.Codigo).ToListAsync(ct);
        if (plantillas.Count == 0) throw new InvalidOperationException("No existe una plantilla de plan contable configurada.");
        var codigosExistentes = await _db.CuentasContables.Where(x => x.EmpresaId == empresaId).Select(x => x.Codigo).ToListAsync(ct);
        var nuevas = plantillas.Where(x => !codigosExistentes.Contains(x.Codigo, StringComparer.OrdinalIgnoreCase)).Select(x => CuentaContable.CopiarParaEmpresa(empresaId, x)).ToList();
        if (nuevas.Count == 0) return;
        _db.CuentasContables.AddRange(nuevas);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<CuentaContableDto> CrearCuentaAsync(int empresaId, CuentaContableCreateDto request, CancellationToken ct = default)
    {
        if (!await _db.Empresas.AnyAsync(x => x.Id == empresaId, ct)) throw new ArgumentException("La empresa indicada no existe.");
        var codigo = request.Codigo?.Trim() ?? string.Empty;
        if (await _db.CuentasContables.AnyAsync(x => x.EmpresaId == empresaId && x.Codigo == codigo, ct)) throw new ArgumentException("Ya existe una cuenta con ese código en esta empresa.");
        var cuenta = CuentaContable.CrearParaEmpresa(empresaId, codigo, request.Nombre, request.Tipo);
        _db.CuentasContables.Add(cuenta);
        await _db.SaveChangesAsync(ct);
        return Map(cuenta);
    }

    public async Task<CuentaContableDto> CambiarEstadoCuentaAsync(int empresaId, int cuentaId, CambiarEstadoCuentaDto request, CancellationToken ct = default)
    {
        var cuenta = await _db.CuentasContables.FirstOrDefaultAsync(x => x.Id == cuentaId && x.EmpresaId == empresaId, ct) ?? throw new ArgumentException("La cuenta no existe en esta empresa.");
        if (!request.Activa && await CuentaTieneMovimientosAsync(empresaId, cuentaId, ct)) throw new ArgumentException("No puedes desactivar una cuenta que ya tiene movimientos. Crea una cuenta nueva para futuras operaciones.");
        cuenta.CambiarEstado(request.Activa);
        await _db.SaveChangesAsync(ct);
        return Map(cuenta);
    }

    public async Task<IReadOnlyList<AsientoDto>> GetAsientosAsync(int empresaId, CancellationToken ct = default)
    {
        var items = await _db.AsientosContables.AsNoTracking().Include(x => x.Detalles).ThenInclude(x => x.CuentaContable)
            .Where(x => x.EmpresaId == empresaId).OrderByDescending(x => x.Fecha).ThenByDescending(x => x.Id).ToListAsync(ct);
        return items.Select(Map).ToList();
    }

    public async Task<AsientoDto> CrearAsientoAsync(int empresaId, AsientoCreateDto request, CancellationToken ct = default)
    {
        await _cierres.VerificarPeriodoAbiertoAsync(empresaId, request.Fecha, ct);
        if (!await _db.Empresas.AnyAsync(x => x.Id == empresaId, ct)) throw new ArgumentException("La empresa indicada no existe.");
        var ids = request.Detalles.Select(x => x.CuentaContableId).Distinct().ToList();
        var validIds = await _db.CuentasContables.Where(x => x.EmpresaId == empresaId && ids.Contains(x.Id) && x.Activa).Select(x => x.Id).ToListAsync(ct);
        if (validIds.Count != ids.Count) throw new ArgumentException("Una o más cuentas contables no pertenecen a la empresa o están inactivas.");
        var entity = AsientoContable.Crear(empresaId, request.Fecha, request.Glosa, request.Detalles.Select(x => (x.CuentaContableId, x.Debe, x.Haber)));
        _db.AsientosContables.Add(entity);
        await _db.SaveChangesAsync(ct);
        await _db.Entry(entity).Collection(x => x.Detalles).Query().Include(x => x.CuentaContable).LoadAsync(ct);
        return Map(entity);
    }

    private Task<bool> CuentaTieneMovimientosAsync(int empresaId, int cuentaId, CancellationToken ct)
        => _db.AsientosContables.Where(x => x.EmpresaId == empresaId).SelectMany(x => x.Detalles).AnyAsync(x => x.CuentaContableId == cuentaId, ct);
    private static CuentaContableDto Map(CuentaContable x) => new(x.Id, x.Codigo, x.Nombre, x.Tipo, x.Activa);
    private static AsientoDto Map(AsientoContable x) => new(x.Id, x.Fecha, x.Glosa, x.Detalles.Sum(d => d.Debe), x.Detalles.Sum(d => d.Haber), x.Detalles.Select(d => new AsientoDetalleDto(d.CuentaContableId, $"{d.CuentaContable?.Codigo} - {d.CuentaContable?.Nombre}", d.Debe, d.Haber)).ToList());
}
