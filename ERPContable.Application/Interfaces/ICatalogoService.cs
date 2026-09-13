using ERPContable.Application.Dtos;

namespace ERPContable.Application.Interfaces;

public interface ICatalogoService
{
    Task<CatalogoDto> ObtenerAsync(int empresaId, CancellationToken ct = default);
    Task<CategoriaDto> CrearCategoriaAsync(int empresaId, CategoriaCreateDto request, CancellationToken ct = default);
    Task<UnidadMedidaDto> CrearUnidadAsync(int empresaId, UnidadMedidaCreateDto request, CancellationToken ct = default);
    Task<ImpuestoDto> CrearImpuestoAsync(int empresaId, ImpuestoCreateDto request, CancellationToken ct = default);
    Task<ProductoDto> CrearProductoAsync(int empresaId, ProductoCreateDto request, CancellationToken ct = default);
    Task<CentroCostoDto> CrearCentroCostoAsync(int empresaId, CentroCostoCreateDto request, CancellationToken ct = default);
}

public sealed record CatalogoDto(IReadOnlyList<ProductoDto> Productos, IReadOnlyList<CategoriaDto> Categorias, IReadOnlyList<UnidadMedidaDto> Unidades, IReadOnlyList<ImpuestoDto> Impuestos, IReadOnlyList<CentroCostoDto> CentrosCosto);
