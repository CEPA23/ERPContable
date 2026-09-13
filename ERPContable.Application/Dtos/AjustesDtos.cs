namespace ERPContable.Application.Dtos;
public sealed record AjusteDetalleCreateDto(int CuentaContableId, decimal Debe, decimal Haber);
public sealed record AjusteCreateDto(DateTime Fecha, string Glosa, IReadOnlyList<AjusteDetalleCreateDto> Detalles);
public sealed record AjusteDto(int Id, DateTime Fecha, string Glosa, int AsientoContableId, decimal TotalDebe, decimal TotalHaber, IReadOnlyList<AsientoDetalleDto> Detalles);
