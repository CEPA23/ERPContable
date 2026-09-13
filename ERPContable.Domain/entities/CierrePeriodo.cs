namespace ERPContable.Domain.Entities;

public sealed class CierrePeriodo
{
    private CierrePeriodo() { }

    public int Id { get; private set; }
    public int EmpresaId { get; private set; }
    public int Ejercicio { get; private set; }
    public int Mes { get; private set; }
    public bool Cerrado { get; private set; }
    public DateTime? CerradoEnUtc { get; private set; }
    public string? CerradoPorUsuarioId { get; private set; }
    public DateTime? ReabiertoEnUtc { get; private set; }
    public string? ReabiertoPorUsuarioId { get; private set; }
    public string? Observacion { get; private set; }

    public static CierrePeriodo Cerrar(int empresaId, int ejercicio, int mes, string usuarioId, string? observacion)
    {
        if (mes is < 1 or > 12) throw new ArgumentException("El mes debe estar entre 1 y 12.");
        return new CierrePeriodo { EmpresaId = empresaId, Ejercicio = ejercicio, Mes = mes, Cerrado = true, CerradoEnUtc = DateTime.UtcNow, CerradoPorUsuarioId = usuarioId, Observacion = Normalizar(observacion) };
    }

    public void Reabrir(string usuarioId, string? observacion)
    {
        Cerrado = false;
        ReabiertoEnUtc = DateTime.UtcNow;
        ReabiertoPorUsuarioId = usuarioId;
        Observacion = Normalizar(observacion) ?? Observacion;
    }

    public void Cerrar(string usuarioId, string? observacion)
    {
        Cerrado = true;
        CerradoEnUtc = DateTime.UtcNow;
        CerradoPorUsuarioId = usuarioId;
        ReabiertoEnUtc = null;
        ReabiertoPorUsuarioId = null;
        Observacion = Normalizar(observacion) ?? Observacion;
    }

    private static string? Normalizar(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
