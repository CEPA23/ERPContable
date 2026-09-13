using ERPContable.Application.Dtos;
using ERPContable.Application.Interfaces;
using ERPContable.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPContable.Infrastructure.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly ERPDbContext _db;
    public DashboardService(ERPDbContext db) => _db = db;

    public async Task<DashboardResumenDto> ObtenerResumenAsync(int empresaId, int ejercicio, int mes, CancellationToken cancellationToken = default)
    {
        if (mes is < 1 or > 12) throw new ArgumentException("Mes no válido.");
        var desde = new DateTime(ejercicio, mes, 1);
        var hasta = desde.AddMonths(1);
        var ventasMes = await _db.Ventas.Where(x => x.EmpresaId == empresaId && x.Fecha >= desde && x.Fecha < hasta).SumAsync(x => (decimal?)x.Total, cancellationToken) ?? 0m;
        var comprasMes = await _db.Compras.Where(x => x.EmpresaId == empresaId && x.Fecha >= desde && x.Fecha < hasta).SumAsync(x => (decimal?)x.Total, cancellationToken) ?? 0m;
        var cajaMes = await _db.MovimientosCaja.Where(x => x.EmpresaId == empresaId && x.Fecha >= desde && x.Fecha < hasta).GroupBy(x => x.Tipo).Select(x => new { Tipo = x.Key, Total = x.Sum(y => y.Monto) }).ToListAsync(cancellationToken);
        var fromMonth = Math.Max(1, mes - 5);
        var meses = Enumerable.Range(fromMonth, mes - fromMonth + 1).ToArray();
        var ventas = await _db.Ventas.Where(x => x.EmpresaId == empresaId && x.Fecha.Year == ejercicio && meses.Contains(x.Fecha.Month)).GroupBy(x => x.Fecha.Month).Select(x => new { Mes = x.Key, Total = x.Sum(y => y.Total) }).ToDictionaryAsync(x => x.Mes, x => x.Total, cancellationToken);
        var compras = await _db.Compras.Where(x => x.EmpresaId == empresaId && x.Fecha.Year == ejercicio && meses.Contains(x.Fecha.Month)).GroupBy(x => x.Fecha.Month).Select(x => new { Mes = x.Key, Total = x.Sum(y => y.Total) }).ToDictionaryAsync(x => x.Mes, x => x.Total, cancellationToken);
        var caja = await _db.MovimientosCaja.Where(x => x.EmpresaId == empresaId && x.Fecha.Year == ejercicio && meses.Contains(x.Fecha.Month)).GroupBy(x => new { x.Fecha.Month, x.Tipo }).Select(x => new { x.Key.Month, x.Key.Tipo, Total = x.Sum(y => y.Monto) }).ToListAsync(cancellationToken);
        var labels = new[] { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };
        var tendencia = meses.Select(numero => new DashboardSerieDto(labels[numero - 1], ventas.GetValueOrDefault(numero), compras.GetValueOrDefault(numero), caja.Where(x => x.Month == numero && x.Tipo.Equals("INGRESO", StringComparison.OrdinalIgnoreCase)).Sum(x => x.Total), caja.Where(x => x.Month == numero && x.Tipo.Equals("EGRESO", StringComparison.OrdinalIgnoreCase)).Sum(x => x.Total))).ToList();
        return new DashboardResumenDto(ventasMes, comprasMes, cajaMes.Where(x => x.Tipo.Equals("INGRESO", StringComparison.OrdinalIgnoreCase)).Sum(x => x.Total), cajaMes.Where(x => x.Tipo.Equals("EGRESO", StringComparison.OrdinalIgnoreCase)).Sum(x => x.Total), tendencia);
    }
}
