using ERPContable.Application.Dtos;
namespace ERPContable.Application.Interfaces;
public interface IAjustesService
{
    Task<IReadOnlyList<AjusteDto>> ObtenerAsync(int empresaId, CancellationToken ct = default);
    Task<AjusteDto> CrearAsync(int empresaId, AjusteCreateDto request, CancellationToken ct = default);
}
