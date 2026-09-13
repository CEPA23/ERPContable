namespace ERPContable.Domain.Entities;

public class ActivoFijo
{
    private ActivoFijo() { }
    public int Id { get; private set; }
    public int EmpresaId { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public DateTime FechaAdquisicion { get; private set; }
    public decimal ValorAdquisicion { get; private set; }
    public int VidaUtilMeses { get; private set; }
    public decimal DepreciacionAcumulada { get; private set; }
    public int CuentaActivoId { get; private set; }
    public int CuentaDepreciacionId { get; private set; }
    public int CuentaGastoId { get; private set; }
    public bool Activo { get; private set; } = true;

    public static ActivoFijo Crear(int empresaId, string codigo, string descripcion, DateTime fechaAdquisicion, decimal valor, int vidaUtilMeses, int cuentaActivoId, int cuentaDepreciacionId, int cuentaGastoId)
    {
        if (empresaId <= 0 || string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(descripcion) || valor <= 0 || vidaUtilMeses <= 0 || cuentaActivoId <= 0 || cuentaDepreciacionId <= 0 || cuentaGastoId <= 0) throw new ArgumentException("Completa todos los datos del activo.");
        return new ActivoFijo { EmpresaId = empresaId, Codigo = codigo.Trim(), Descripcion = descripcion.Trim(), FechaAdquisicion = fechaAdquisicion.Date, ValorAdquisicion = decimal.Round(valor, 2), VidaUtilMeses = vidaUtilMeses, CuentaActivoId = cuentaActivoId, CuentaDepreciacionId = cuentaDepreciacionId, CuentaGastoId = cuentaGastoId };
    }
    public decimal DepreciacionMensual => decimal.Round(ValorAdquisicion / VidaUtilMeses, 2);
    public void RegistrarDepreciacion(decimal importe) { if (importe <= 0 || DepreciacionAcumulada + importe > ValorAdquisicion) throw new ArgumentException("La depreciación supera el valor del activo."); DepreciacionAcumulada += importe; }
}
