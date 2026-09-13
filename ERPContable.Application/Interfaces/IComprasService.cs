using ERPContable.Application.Dtos;

namespace ERPContable.Application.Interfaces;

public interface IComprasService
{
    Task<IReadOnlyList<ProveedorDto>> ObtenerProveedoresAsync(int empresaId, CancellationToken ct = default);
    Task<ProveedorDto> CrearProveedorAsync(int empresaId, ProveedorCreateDto request, CancellationToken ct = default);
    Task<IReadOnlyList<CompraDto>> ObtenerComprasAsync(int empresaId, CancellationToken ct = default);
    Task<CompraDto> CrearCompraAsync(int empresaId, CompraCreateDto request, CancellationToken ct = default);
}
