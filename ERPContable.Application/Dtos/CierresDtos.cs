namespace ERPContable.Application.Dtos;

public sealed record CierrePeriodoDto(int Id, int Ejercicio, int Mes, bool Cerrado, DateTime? CerradoEnUtc, DateTime? ReabiertoEnUtc, string? Observacion);
public sealed record CierreEjercicioDto(int Id, int Ejercicio, bool Cerrado, DateTime? CerradoEnUtc, DateTime? ReabiertoEnUtc, string? Observacion, int? AsientoCierreId, int? AsientoAperturaId);
public sealed record EstadoCierresDto(int Ejercicio, IReadOnlyList<CierrePeriodoDto> Periodos, CierreEjercicioDto? CierreEjercicio, int AsientosEjercicio);
public sealed record CerrarPeriodoDto(string? Observacion);
public sealed record CerrarEjercicioDto(string? Observacion);
public sealed record ConfiguracionContableDto(int EmpresaId, int? CuentaResultadoAcumuladoId, string? CuentaResultadoAcumulado, int? CuentaInventarioId, string? CuentaInventario, int? CuentaCostoVentasId, string? CuentaCostoVentas);
public sealed record ActualizarConfiguracionContableDto(int CuentaResultadoAcumuladoId, int CuentaInventarioId, int CuentaCostoVentasId);
