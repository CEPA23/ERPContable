namespace ERPContable.Application.Dtos;

public sealed record ReportePeriodoDto(DateTime Desde, DateTime Hasta);

public sealed record LibroDiarioLineaDto(
    DateTime Fecha,
    int AsientoId,
    string Glosa,
    string CodigoCuenta,
    string NombreCuenta,
    decimal Debe,
    decimal Haber);

public sealed record LibroDiarioDto(
    DateTime Desde,
    DateTime Hasta,
    IReadOnlyList<LibroDiarioLineaDto> Lineas,
    decimal TotalDebe,
    decimal TotalHaber);

public sealed record LibroMayorMovimientoDto(
    DateTime Fecha,
    int AsientoId,
    string Glosa,
    decimal Debe,
    decimal Haber,
    decimal Saldo);

public sealed record LibroMayorCuentaDto(
    int CuentaId,
    string Codigo,
    string Nombre,
    string Tipo,
    decimal TotalDebe,
    decimal TotalHaber,
    decimal SaldoDeudor,
    decimal SaldoAcreedor,
    IReadOnlyList<LibroMayorMovimientoDto> Movimientos);

public sealed record BalanceComprobacionLineaDto(
    int CuentaId,
    string Codigo,
    string Nombre,
    string Tipo,
    decimal Debe,
    decimal Haber,
    decimal SaldoDeudor,
    decimal SaldoAcreedor);

public sealed record BalanceComprobacionDto(
    DateTime Desde,
    DateTime Hasta,
    IReadOnlyList<BalanceComprobacionLineaDto> Lineas,
    decimal TotalDebe,
    decimal TotalHaber,
    decimal TotalSaldoDeudor,
    decimal TotalSaldoAcreedor);

public sealed record EstadoResultadosDto(
    DateTime Desde,
    DateTime Hasta,
    decimal Ingresos,
    decimal Gastos,
    decimal Resultado);

public sealed record BalanceGeneralDto(
    DateTime Hasta,
    decimal Activos,
    decimal Pasivos,
    decimal Patrimonio,
    decimal ResultadoDelPeriodo,
    decimal TotalPasivoPatrimonio);

public sealed record FlujoCajaDto(
    DateTime Desde,
    DateTime Hasta,
    decimal Entradas,
    decimal Salidas,
    decimal FlujoNeto,
    IReadOnlyList<FlujoCajaMovimientoDto> Movimientos);

public sealed record FlujoCajaMovimientoDto(
    DateTime Fecha,
    int AsientoId,
    string Glosa,
    string CodigoCuenta,
    string NombreCuenta,
    decimal Entrada,
    decimal Salida);
