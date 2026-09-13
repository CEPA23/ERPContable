namespace ERPContable.Domain.Entities;

public sealed record CuentaPlantilla(string Codigo, string Nombre, string Tipo);

public static class PlanContableBase
{
    // Base PCGE operativa. Cada empresa recibe copias independientes de estas cuentas.
    public static IReadOnlyList<CuentaPlantilla> Cuentas { get; } =
    [
        new("10", "Efectivo y equivalentes de efectivo", "ACTIVO"),
        new("101", "Caja", "ACTIVO"),
        new("104", "Cuentas corrientes en instituciones financieras", "ACTIVO"),
        new("12", "Cuentas por cobrar comerciales", "ACTIVO"),
        new("20", "Mercaderías", "ACTIVO"),
        new("33", "Propiedad, planta y equipo", "ACTIVO"),
        new("39", "Depreciación acumulada", "ACTIVO"),
        new("40", "Tributos por pagar", "PASIVO"),
        new("4011", "IGV por pagar", "PASIVO"),
        new("42", "Cuentas por pagar comerciales", "PASIVO"),
        new("50", "Capital", "PATRIMONIO"),
        new("59", "Resultados acumulados", "PATRIMONIO"),
        new("60", "Compras", "GASTO"),
        new("63", "Gastos de servicios prestados por terceros", "GASTO"),
        new("68", "Valuación y deterioro de activos", "GASTO"),
        new("69", "Costo de ventas", "GASTO"),
        new("70", "Ventas", "INGRESO"),
        new("75", "Otros ingresos de gestión", "INGRESO")
    ];
}
