using ERPContable.Application.Dtos;

namespace ERPContable.Application.Interfaces;

public interface IContabilidadService
{
    Task<IReadOnlyList<CuentaContableDto>> GetCuentasAsync(int empresaId, CancellationToken cancellationToken = default);
    Task InicializarPlanContableAsync(int empresaId, CancellationToken cancellationToken = default);
    Task<CuentaContableDto> CrearCuentaAsync(int empresaId, CuentaContableCreateDto request, CancellationToken cancellationToken = default);
    Task<CuentaContableDto> CambiarEstadoCuentaAsync(int empresaId, int cuentaId, CambiarEstadoCuentaDto request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AsientoDto>> GetAsientosAsync(int empresaId, CancellationToken cancellationToken = default);
    Task<AsientoDto> CrearAsientoAsync(int empresaId, AsientoCreateDto request, CancellationToken cancellationToken = default);
}
