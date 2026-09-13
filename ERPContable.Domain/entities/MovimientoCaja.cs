namespace ERPContable.Domain.Entities;

public class MovimientoCaja
{
    private MovimientoCaja() { }
    public int Id { get; private set; }
    public int EmpresaId { get; private set; }
    public DateTime Fecha { get; private set; }
    public string Tipo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public decimal Monto { get; private set; }
    public int CuentaCajaId { get; private set; }
    public int CuentaContrapartidaId { get; private set; }
    public int? AsientoContableId { get; private set; }

    public static MovimientoCaja Crear(int empresaId, DateTime fecha, string tipo, string descripcion, decimal monto, int cuentaCajaId, int cuentaContrapartidaId)
    {
        if (empresaId <= 0 || string.IsNullOrWhiteSpace(descripcion) || monto <= 0 || cuentaCajaId <= 0 || cuentaContrapartidaId <= 0)
            throw new ArgumentException("Completa los datos del movimiento de caja.");
        if (!string.Equals(tipo, "INGRESO", StringComparison.OrdinalIgnoreCase) && !string.Equals(tipo, "EGRESO", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("El tipo debe ser INGRESO o EGRESO.");
        return new MovimientoCaja { EmpresaId = empresaId, Fecha = fecha.Date, Tipo = tipo.Trim().ToUpperInvariant(), Descripcion = descripcion.Trim(), Monto = decimal.Round(monto, 2), CuentaCajaId = cuentaCajaId, CuentaContrapartidaId = cuentaContrapartidaId };
    }

    public void AsignarAsiento(int asientoId) => AsientoContableId = asientoId;
}
