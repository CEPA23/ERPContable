namespace ERPContable.Application.Dtos;

public sealed record InventarioProductoDto(int Id, int ProductoId, string Codigo, string Producto, string Unidad, decimal StockActual, decimal StockMinimo, decimal CostoPromedio, bool BajoMinimo);
public sealed record MovimientoInventarioDto(int Id, int ProductoId, string Codigo, string Producto, string Tipo, decimal Cantidad, decimal CostoUnitario, decimal StockAnterior, decimal StockPosterior, string? Referencia, string? Observacion, DateTime FechaUtc);
public sealed record MovimientoInventarioCreateDto(int ProductoId, string Tipo, decimal Cantidad, decimal? StockObjetivo, decimal CostoUnitario, string? Referencia, string? Observacion, DateTime? Fecha = null);
public sealed record StockMinimoUpdateDto(decimal StockMinimo);
public sealed record InventarioDto(IReadOnlyList<InventarioProductoDto> Productos, IReadOnlyList<MovimientoInventarioDto> Movimientos);
