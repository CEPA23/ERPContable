namespace ERPContable.Application.Dtos;

public sealed record CuentaContableDto(int Id, string Codigo, string Nombre, string Tipo, bool Activa);
public sealed record CuentaContableCreateDto(string Codigo, string Nombre, string Tipo);
public sealed record CambiarEstadoCuentaDto(bool Activa);
public sealed record AsientoDetalleCreateDto(int CuentaContableId, decimal Debe, decimal Haber);
public sealed record AsientoCreateDto(DateTime Fecha, string Glosa, IReadOnlyList<AsientoDetalleCreateDto> Detalles);
public sealed record AsientoDetalleDto(int CuentaContableId, string Cuenta, decimal Debe, decimal Haber);
public sealed record AsientoDto(int Id, DateTime Fecha, string Glosa, decimal TotalDebe, decimal TotalHaber, IReadOnlyList<AsientoDetalleDto> Detalles);
