using ERPContable.Application.Dtos;

namespace ERPContable.Application.Interfaces;

public interface IReportesContablesService
{
    Task<LibroDiarioDto> ObtenerLibroDiarioAsync(int empresaId, DateTime desde, DateTime hasta, CancellationToken ct = default);
    Task<IReadOnlyList<LibroMayorCuentaDto>> ObtenerLibroMayorAsync(int empresaId, DateTime desde, DateTime hasta, CancellationToken ct = default);
    Task<BalanceComprobacionDto> ObtenerBalanceComprobacionAsync(int empresaId, DateTime desde, DateTime hasta, CancellationToken ct = default);
    Task<EstadoResultadosDto> ObtenerEstadoResultadosAsync(int empresaId, DateTime desde, DateTime hasta, CancellationToken ct = default);
    Task<BalanceGeneralDto> ObtenerBalanceGeneralAsync(int empresaId, DateTime hasta, CancellationToken ct = default);
    Task<FlujoCajaDto> ObtenerFlujoCajaAsync(int empresaId, DateTime desde, DateTime hasta, CancellationToken ct = default);
}
