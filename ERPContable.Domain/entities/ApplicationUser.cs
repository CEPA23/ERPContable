using Microsoft.AspNetCore.Identity;

namespace ERPContable.Domain.Entities;

public sealed class ApplicationUser : IdentityUser
{
    public string NombreCompleto { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public DateTime CreadoEnUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UltimoAccesoEnUtc { get; set; }
    public ICollection<UsuarioEmpresa> Empresas { get; set; } = [];
}
