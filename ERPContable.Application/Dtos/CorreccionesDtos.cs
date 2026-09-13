namespace ERPContable.Application.Dtos;

public sealed record CorreccionLineaRequest(int DetalleId, decimal Cantidad);
public sealed record CorreccionRequest(Guid SolicitudId, string Tipo, DateTime Fecha, string Motivo, IReadOnlyList<CorreccionLineaRequest> Detalles);
public sealed record CorreccionLineaDisponible(int Id, string Descripcion, decimal Cantidad, decimal Devuelta, decimal Disponible, decimal PrecioUnitario);
public sealed record CorreccionHistorialDto(int Id, string Tipo, DateTime Fecha, string Motivo, string Usuario, DateTime CreadoEnUtc, decimal Subtotal, decimal Igv, int? AsientoContableId, int? AsientoCostoId, IReadOnlyList<CorreccionLineaHistorialDto> Detalles);
public sealed record CorreccionLineaHistorialDto(int DetalleId, decimal Cantidad, decimal Subtotal, decimal CostoInventario, int? MovimientoInventarioId);
public sealed record CorreccionesDocumentoDto(int DocumentoId, string Comprobante, DateTime Fecha, string Estado, decimal TotalOriginal, decimal TotalRevertido, IReadOnlyList<CorreccionLineaDisponible> Detalles, IReadOnlyList<CorreccionHistorialDto> Historial);
