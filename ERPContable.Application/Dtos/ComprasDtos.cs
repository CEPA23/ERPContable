namespace ERPContable.Application.Dtos;

public sealed record ProveedorDto(int Id, string Documento, string RazonSocial, string? Direccion, string? Email, bool Activo);
public sealed record ProveedorCreateDto(string Documento, string RazonSocial, string? Direccion, string? Email);
public sealed record CompraDetalleCreateDto(int CuentaContableId, string Descripcion, decimal Cantidad, decimal PrecioUnitario, int? ProductoServicioId = null);
public sealed record CompraCreateDto(DateTime Fecha, int ProveedorId, string TipoComprobante, string Serie, string Numero, decimal Igv, string FormaPago, int CuentaContrapartidaId, int CuentaIgvId, IReadOnlyList<CompraDetalleCreateDto> Detalles);
public sealed record CompraDetalleDto(string Cuenta, string Descripcion, decimal Cantidad, decimal PrecioUnitario, decimal Total, int? ProductoServicioId = null, string? Producto = null);
public sealed record CompraDto(int Id, DateTime Fecha, string Proveedor, string Comprobante, decimal Subtotal, decimal Igv, decimal Total, string FormaPago, int? AsientoContableId, IReadOnlyList<CompraDetalleDto> Detalles, string Estado = "VIGENTE", decimal SubtotalRevertido = 0, decimal IgvRevertido = 0);
