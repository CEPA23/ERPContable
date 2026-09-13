using ERPContable.Application.Dtos;
using ERPContable.Application.Interfaces;
using ERPContable.Domain.Entities;
using ERPContable.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPContable.Infrastructure.Services;

public sealed class AjustesService : IAjustesService
{
    private readonly ERPDbContext _db;
    private readonly ICierreContableService _cierres;
    public AjustesService(ERPDbContext db, ICierreContableService cierres) { _db = db; _cierres = cierres; }
    public async Task<IReadOnlyList<AjusteDto>> ObtenerAsync(int empresaId, CancellationToken ct = default)
    {
        var items = await _db.AjustesContables.AsNoTracking().Where(x => x.EmpresaId == empresaId).OrderByDescending(x => x.Fecha).ThenByDescending(x => x.Id).ToListAsync(ct); var ids = items.Select(x => x.AsientoContableId).ToList(); var asientos = await _db.AsientosContables.AsNoTracking().Include(x => x.Detalles).ThenInclude(x => x.CuentaContable).Where(x => ids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct); return items.Where(x => asientos.ContainsKey(x.AsientoContableId)).Select(x => Map(x, asientos[x.AsientoContableId])).ToList();
    }
    public async Task<AjusteDto> CrearAsync(int empresaId, AjusteCreateDto request, CancellationToken ct = default)
    {
        await _cierres.VerificarPeriodoAbiertoAsync(empresaId, request.Fecha, ct);
        if (!await _db.Empresas.AnyAsync(x => x.Id == empresaId, ct)) throw new ArgumentException("La empresa indicada no existe."); var ids = request.Detalles.Select(x => x.CuentaContableId).Distinct().ToList(); var cuentas = await _db.CuentasContables.Where(x => x.EmpresaId == empresaId && ids.Contains(x.Id) && x.Activa).ToListAsync(ct); if (cuentas.Count != ids.Count) throw new ArgumentException("Una o más cuentas no pertenecen a la empresa o están inactivas."); var asiento = AsientoContable.Crear(empresaId, request.Fecha, request.Glosa, request.Detalles.Select(x => (x.CuentaContableId, x.Debe, x.Haber))); await using var transaction = await _db.Database.BeginTransactionAsync(ct); _db.AsientosContables.Add(asiento); await _db.SaveChangesAsync(ct); var ajuste = AjusteContable.Crear(empresaId, request.Fecha, request.Glosa, asiento.Id); _db.AjustesContables.Add(ajuste); await _db.SaveChangesAsync(ct); await transaction.CommitAsync(ct); await _db.Entry(asiento).Collection(x => x.Detalles).Query().Include(x => x.CuentaContable).LoadAsync(ct); return Map(ajuste, asiento);
    }
    private static AjusteDto Map(AjusteContable ajuste, AsientoContable asiento) => new(ajuste.Id, ajuste.Fecha, ajuste.Glosa, ajuste.AsientoContableId, asiento.Detalles.Sum(x => x.Debe), asiento.Detalles.Sum(x => x.Haber), asiento.Detalles.Select(x => new AsientoDetalleDto(x.CuentaContableId, $"{x.CuentaContable?.Codigo} - {x.CuentaContable?.Nombre}", x.Debe, x.Haber)).ToList());
}
