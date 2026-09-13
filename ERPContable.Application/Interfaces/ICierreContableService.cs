using ERPContable.Application.Dtos;

namespace ERPContable.Application.Interfaces;

public interface ICierreContableService
{
    Task<EstadoCierresDto> ObtenerEstadoAsync(int empresaId, int ejercicio, CancellationToken cancellationToken = default);
    Task<CierrePeriodoDto> CerrarPeriodoAsync(int empresaId, int ejercicio, int mes, string usuarioId, CerrarPeriodoDto request, CancellationToken cancellationToken = default);
    Task<CierrePeriodoDto> ReabrirPeriodoAsync(int empresaId, int ejercicio, int mes, string usuarioId, CerrarPeriodoDto request, CancellationToken cancellationToken = default);
    Task<CierreEjercicioDto> CerrarEjercicioAsync(int empresaId, int ejercicio, string usuarioId, CerrarEjercicioDto request, CancellationToken cancellationToken = default);
    Task<CierreEjercicioDto> ReabrirEjercicioAsync(int empresaId, int ejercicio, string usuarioId, CerrarEjercicioDto request, CancellationToken cancellationToken = default);
    Task VerificarPeriodoAbiertoAsync(int empresaId, DateTime fecha, CancellationToken cancellationToken = default);
}
