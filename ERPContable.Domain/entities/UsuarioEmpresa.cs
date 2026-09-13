namespace ERPContable.Domain.Entities;

public sealed class UsuarioEmpresa
{
    public string UsuarioId { get; set; } = string.Empty;
    public ApplicationUser Usuario { get; set; } = null!;
    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;
    public string Rol { get; set; } = RolesERP.Contador;
    public DateTime CreadoEnUtc { get; set; } = DateTime.UtcNow;
    public bool Activo { get; set; } = true;
}

public static class RolesERP
{
    public const string Administrador = "Administrador";
    public const string Contador = "Contador";
    public const string Cajero = "Cajero";
    public const string Gerente = "Gerente";
    public const string Auditor = "Auditor";
}
