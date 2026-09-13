using ERPContable.Application.Dtos;
namespace ERPContable.Application.Interfaces;
public interface IActivosService
{
    Task<IReadOnlyList<ActivoDto>> ObtenerAsync(int empresaId, CancellationToken ct = default);
    Task<ActivoDto> CrearAsync(int empresaId, ActivoCreateDto request, CancellationToken ct = default);
    Task<ActivoDto> DepreciarAsync(int empresaId, int activoId, DepreciarActivoDto request, CancellationToken ct = default);
}
