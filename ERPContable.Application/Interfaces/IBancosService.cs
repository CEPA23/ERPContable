using ERPContable.Application.Dtos;
namespace ERPContable.Application.Interfaces;
public interface IBancosService
{
    Task<IReadOnlyList<CuentaBancariaDto>> ObtenerCuentasAsync(int empresaId, CancellationToken ct = default);
    Task<CuentaBancariaDto> CrearCuentaAsync(int empresaId, CuentaBancariaCreateDto request, CancellationToken ct = default);
    Task<IReadOnlyList<MovimientoBancoDto>> ObtenerMovimientosAsync(int empresaId, CancellationToken ct = default);
    Task<MovimientoBancoDto> CrearMovimientoAsync(int empresaId, MovimientoBancoCreateDto request, CancellationToken ct = default);
}
