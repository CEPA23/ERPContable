namespace ERPContable.Domain.Entities;

public sealed class ConfiguracionContableEmpresa
{
    private ConfiguracionContableEmpresa() { }

    public int Id { get; private set; }
    public int EmpresaId { get; private set; }
    public int CuentaResultadoAcumuladoId { get; private set; }
    public int? CuentaInventarioId { get; private set; }
    public int? CuentaCostoVentasId { get; private set; }
    public DateTime ActualizadoEnUtc { get; private set; }

    public static ConfiguracionContableEmpresa Crear(int empresaId, int cuentaResultadoAcumuladoId, int? cuentaInventarioId = null, int? cuentaCostoVentasId = null)
        => new()
        {
            EmpresaId = empresaId,
            CuentaResultadoAcumuladoId = cuentaResultadoAcumuladoId,
            CuentaInventarioId = cuentaInventarioId,
            CuentaCostoVentasId = cuentaCostoVentasId,
            ActualizadoEnUtc = DateTime.UtcNow
        };

    public void Actualizar(int cuentaResultadoAcumuladoId, int cuentaInventarioId, int cuentaCostoVentasId)
    {
        CuentaResultadoAcumuladoId = cuentaResultadoAcumuladoId;
        CuentaInventarioId = cuentaInventarioId;
        CuentaCostoVentasId = cuentaCostoVentasId;
        ActualizadoEnUtc = DateTime.UtcNow;
    }
}
