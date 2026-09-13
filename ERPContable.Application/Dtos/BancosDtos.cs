namespace ERPContable.Application.Dtos;
public sealed record CuentaBancariaCreateDto(string Banco, string NumeroCuenta, string Moneda, int CuentaContableId);
public sealed record CuentaBancariaDto(int Id, string Banco, string NumeroCuenta, string Moneda, string CuentaContable, bool Activa);
public sealed record MovimientoBancoCreateDto(int CuentaBancariaId, DateTime Fecha, string Tipo, string Descripcion, decimal Monto, int CuentaContrapartidaId);
public sealed record MovimientoBancoDto(int Id, DateTime Fecha, string Banco, string NumeroCuenta, string Tipo, string Descripcion, decimal Monto, string CuentaContrapartida, int? AsientoContableId);
