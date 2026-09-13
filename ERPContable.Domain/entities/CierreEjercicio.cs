namespace ERPContable.Domain.Entities;

public sealed class CierreEjercicio
{
    private CierreEjercicio() { }

    public int Id { get; private set; }
    public int EmpresaId { get; private set; }
    public int Ejercicio { get; private set; }
    public bool Cerrado { get; private set; }
    public DateTime? CerradoEnUtc { get; private set; }
    public string? CerradoPorUsuarioId { get; private set; }
    public DateTime? ReabiertoEnUtc { get; private set; }
    public string? ReabiertoPorUsuarioId { get; private set; }
    public string? Observacion { get; private set; }
    public int? AsientoCierreId { get; private set; }
    public int? AsientoAperturaId { get; private set; }

    public static CierreEjercicio Cerrar(int empresaId, int ejercicio, string usuarioId, string? observacion)
        => new() { EmpresaId = empresaId, Ejercicio = ejercicio, Cerrado = true, CerradoEnUtc = DateTime.UtcNow, CerradoPorUsuarioId = usuarioId, Observacion = Normalizar(observacion) };

    public void Reabrir(string usuarioId, string? observacion)
    {
        Cerrado = false;
        ReabiertoEnUtc = DateTime.UtcNow;
        ReabiertoPorUsuarioId = usuarioId;
        Observacion = Normalizar(observacion) ?? Observacion;
    }

    public void RegistrarAsientosAutomaticos(int? asientoCierreId, int? asientoAperturaId)
    {
        AsientoCierreId = asientoCierreId;
        AsientoAperturaId = asientoAperturaId;
    }

    public void Cerrar(string usuarioId, string? observacion)
    {
        Cerrado = true;
        CerradoEnUtc = DateTime.UtcNow;
        CerradoPorUsuarioId = usuarioId;
        ReabiertoEnUtc = null;
        ReabiertoPorUsuarioId = null;
        Observacion = Normalizar(observacion) ?? Observacion;
        AsientoCierreId = null;
        AsientoAperturaId = null;
    }

    private static string? Normalizar(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
