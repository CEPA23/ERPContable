namespace ERPContable.Application.Dtos;

public sealed record DashboardSerieDto(string Etiqueta, decimal Ventas, decimal Compras, decimal IngresosCaja, decimal EgresosCaja);
public sealed record DashboardResumenDto(decimal VentasMes, decimal ComprasMes, decimal IngresosCajaMes, decimal EgresosCajaMes, IReadOnlyList<DashboardSerieDto> Tendencia);
