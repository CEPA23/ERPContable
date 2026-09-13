namespace ERPContable.Domain.Entities;

public sealed class RefreshToken
{
    public long Id { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
    public ApplicationUser Usuario { get; set; } = null!;
    public string TokenHash { get; set; } = string.Empty;
    public int? EmpresaId { get; set; }
    public string? RolEmpresa { get; set; }
    public DateTime CreadoEnUtc { get; set; } = DateTime.UtcNow;
    public DateTime ExpiraEnUtc { get; set; }
    public DateTime? RevocadoEnUtc { get; set; }
    public string? ReemplazadoPorHash { get; set; }
    public string? DireccionIp { get; set; }
}
