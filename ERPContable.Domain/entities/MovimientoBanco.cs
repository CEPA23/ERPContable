namespace ERPContable.Domain.Entities;

public class MovimientoBanco
{
    private MovimientoBanco() { }
    public int Id { get; private set; }
    public int EmpresaId { get; private set; }
    public int CuentaBancariaId { get; private set; }
    public CuentaBancaria? CuentaBancaria { get; private set; }
    public DateTime Fecha { get; private set; }
    public string Tipo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public decimal Monto { get; private set; }
    public int CuentaContrapartidaId { get; private set; }
    public int? AsientoContableId { get; private set; }

    public static MovimientoBanco Crear(int empresaId, int cuentaBancariaId, DateTime fecha, string tipo, string descripcion, decimal monto, int cuentaContrapartidaId)
    {
        if (empresaId <= 0 || cuentaBancariaId <= 0 || string.IsNullOrWhiteSpace(descripcion) || monto <= 0 || cuentaContrapartidaId <= 0)
            throw new ArgumentException("Completa los datos del movimiento bancario.");
        if (!string.Equals(tipo, "INGRESO", StringComparison.OrdinalIgnoreCase) && !string.Equals(tipo, "EGRESO", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("El tipo debe ser INGRESO o EGRESO.");
        return new MovimientoBanco { EmpresaId = empresaId, CuentaBancariaId = cuentaBancariaId, Fecha = fecha.Date, Tipo = tipo.Trim().ToUpperInvariant(), Descripcion = descripcion.Trim(), Monto = decimal.Round(monto, 2), CuentaContrapartidaId = cuentaContrapartidaId };
    }
    public void AsignarAsiento(int asientoId) => AsientoContableId = asientoId;
}
