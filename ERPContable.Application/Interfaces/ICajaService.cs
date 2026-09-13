using ERPContable.Application.Dtos;
namespace ERPContable.Application.Interfaces;
public interface ICajaService
{
    Task<IReadOnlyList<MovimientoCajaDto>> ObtenerMovimientosAsync(int empresaId, CancellationToken ct = default);
    Task<MovimientoCajaDto> CrearMovimientoAsync(int empresaId, MovimientoCajaCreateDto request, CancellationToken ct = default);
}
