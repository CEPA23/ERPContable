namespace ERPContable.Application.Dtos;
public sealed record ClienteDto(int Id, string Documento, string RazonSocial, string? Direccion, string? Email, bool Activo);
public sealed record ClienteCreateDto(string Documento, string RazonSocial, string? Direccion, string? Email);
public sealed record VentaDetalleCreateDto(int CuentaContableId, string Descripcion, decimal Cantidad, decimal PrecioUnitario, int? ProductoServicioId = null);
public sealed record VentaCreateDto(DateTime Fecha, int ClienteId, string TipoComprobante, string Serie, string Numero, decimal Igv, string FormaPago, int CuentaContrapartidaId, int CuentaIgvId, IReadOnlyList<VentaDetalleCreateDto> Detalles);
public sealed record VentaDetalleDto(string Cuenta, string Descripcion, decimal Cantidad, decimal PrecioUnitario, decimal Total, int? ProductoServicioId = null, string? Producto = null);
public sealed record VentaDto(int Id, DateTime Fecha, string Cliente, string Comprobante, decimal Subtotal, decimal Igv, decimal Total, string FormaPago, int? AsientoContableId, IReadOnlyList<VentaDetalleDto> Detalles, int? AsientoCostoVentaId = null, string Estado = "VIGENTE", decimal SubtotalRevertido = 0, decimal IgvRevertido = 0);
