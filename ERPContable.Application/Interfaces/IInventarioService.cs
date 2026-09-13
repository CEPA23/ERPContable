using ERPContable.Application.Dtos;

namespace ERPContable.Application.Interfaces;

public interface IInventarioService
{
    Task<InventarioDto> ObtenerAsync(int empresaId, int? productoId = null, CancellationToken ct = default);
    Task<MovimientoInventarioDto> RegistrarMovimientoAsync(int empresaId, MovimientoInventarioCreateDto request, CancellationToken ct = default);
    Task<InventarioProductoDto> ActualizarStockMinimoAsync(int empresaId, int productoId, StockMinimoUpdateDto request, CancellationToken ct = default);
}
