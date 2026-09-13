namespace ERPContable.Application.Dtos;
public sealed record MovimientoCajaCreateDto(DateTime Fecha, string Tipo, string Descripcion, decimal Monto, int CuentaCajaId, int CuentaContrapartidaId);
public sealed record MovimientoCajaDto(int Id, DateTime Fecha, string Tipo, string Descripcion, decimal Monto, string CuentaCaja, string CuentaContrapartida, int? AsientoContableId);
