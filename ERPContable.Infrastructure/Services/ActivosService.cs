using ERPContable.Application.Dtos;
using ERPContable.Application.Interfaces;
using ERPContable.Domain.Entities;
using ERPContable.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPContable.Infrastructure.Services;

public sealed class ActivosService : IActivosService
{
    private readonly ERPDbContext _db;
    private readonly ICierreContableService _cierres;
    public ActivosService(ERPDbContext db, ICierreContableService cierres) { _db = db; _cierres = cierres; }
    public async Task<IReadOnlyList<ActivoDto>> ObtenerAsync(int empresaId, CancellationToken ct = default) { var items = await _db.ActivosFijos.AsNoTracking().Where(x => x.EmpresaId == empresaId).OrderBy(x => x.Codigo).ToListAsync(ct); return items.Select(Map).ToList(); }
    public async Task<ActivoDto> CrearAsync(int empresaId, ActivoCreateDto request, CancellationToken ct = default)
    {
        await _cierres.VerificarPeriodoAbiertoAsync(empresaId, request.FechaAdquisicion, ct);
        if (await _db.ActivosFijos.AnyAsync(x => x.EmpresaId == empresaId && x.Codigo == request.Codigo, ct)) throw new ArgumentException("Ya existe un activo con ese código.");
        var ids = new[] { request.CuentaActivoId, request.CuentaDepreciacionId, request.CuentaGastoId }; var cuentas = await _db.CuentasContables.Where(x => x.EmpresaId == empresaId && ids.Contains(x.Id) && x.Activa).ToListAsync(ct); if (cuentas.Count != ids.Distinct().Count()) throw new ArgumentException("Una o más cuentas del activo no pertenecen a la empresa o están inactivas.");
        var entity = ActivoFijo.Crear(empresaId, request.Codigo, request.Descripcion, request.FechaAdquisicion, request.ValorAdquisicion, request.VidaUtilMeses, request.CuentaActivoId, request.CuentaDepreciacionId, request.CuentaGastoId); _db.ActivosFijos.Add(entity); await _db.SaveChangesAsync(ct); return Map(entity);
    }
    public async Task<ActivoDto> DepreciarAsync(int empresaId, int activoId, DepreciarActivoDto request, CancellationToken ct = default)
    {
        await _cierres.VerificarPeriodoAbiertoAsync(empresaId, request.Fecha, ct);
        var activo = await _db.ActivosFijos.FirstOrDefaultAsync(x => x.Id == activoId && x.EmpresaId == empresaId && x.Activo, ct) ?? throw new ArgumentException("El activo no existe o está inactivo."); var importe = request.Importe.GetValueOrDefault(activo.DepreciacionMensual); activo.RegistrarDepreciacion(importe);
        var asiento = AsientoContable.Crear(empresaId, request.Fecha, $"Depreciación {activo.Codigo} - {activo.Descripcion}", new[] { (activo.CuentaGastoId, importe, 0m), (activo.CuentaDepreciacionId, 0m, importe) }); _db.AsientosContables.Add(asiento); await _db.SaveChangesAsync(ct); await _db.SaveChangesAsync(ct); return Map(activo);
    }
    private static ActivoDto Map(ActivoFijo x) => new(x.Id, x.Codigo, x.Descripcion, x.FechaAdquisicion, x.ValorAdquisicion, x.VidaUtilMeses, x.DepreciacionMensual, x.DepreciacionAcumulada, x.ValorAdquisicion - x.DepreciacionAcumulada, x.Activo);
}
