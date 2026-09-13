namespace ERPContable.Application.Dtos;
public sealed record ActivoCreateDto(string Codigo, string Descripcion, DateTime FechaAdquisicion, decimal ValorAdquisicion, int VidaUtilMeses, int CuentaActivoId, int CuentaDepreciacionId, int CuentaGastoId);
public sealed record ActivoDto(int Id, string Codigo, string Descripcion, DateTime FechaAdquisicion, decimal ValorAdquisicion, int VidaUtilMeses, decimal DepreciacionMensual, decimal DepreciacionAcumulada, decimal ValorNeto, bool Activo);
public sealed record DepreciarActivoDto(DateTime Fecha, decimal? Importe);
