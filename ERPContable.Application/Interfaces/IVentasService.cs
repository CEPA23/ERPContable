using ERPContable.Application.Dtos;
namespace ERPContable.Application.Interfaces;
public interface IVentasService
{
    Task<IReadOnlyList<ClienteDto>> ObtenerClientesAsync(int empresaId, CancellationToken ct = default);
    Task<ClienteDto> CrearClienteAsync(int empresaId, ClienteCreateDto request, CancellationToken ct = default);
    Task<IReadOnlyList<VentaDto>> ObtenerVentasAsync(int empresaId, CancellationToken ct = default);
    Task<VentaDto> CrearVentaAsync(int empresaId, VentaCreateDto request, CancellationToken ct = default);
}
