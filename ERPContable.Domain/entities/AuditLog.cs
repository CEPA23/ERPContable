namespace ERPContable.Domain.Entities;

public sealed class AuditLog
{
    public long Id { get; set; }
    public string? UsuarioId { get; set; }
    public int? EmpresaId { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string Entidad { get; set; } = string.Empty;
    public string? Detalles { get; set; }
    public string? DireccionIp { get; set; }
    public DateTime CreadoEnUtc { get; set; } = DateTime.UtcNow;
}
