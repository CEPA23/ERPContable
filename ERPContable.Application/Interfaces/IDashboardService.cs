using ERPContable.Application.Dtos;

namespace ERPContable.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardResumenDto> ObtenerResumenAsync(int empresaId, int ejercicio, int mes, CancellationToken cancellationToken = default);
}
