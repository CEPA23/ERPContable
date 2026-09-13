namespace ERPContable.Domain.Entities;

public class Cliente
{
    private Cliente() { }
    public int Id { get; private set; }
    public int EmpresaId { get; private set; }
    public Empresa? Empresa { get; private set; }
    public string Documento { get; private set; } = string.Empty;
    public string RazonSocial { get; private set; } = string.Empty;
    public string? Direccion { get; private set; }
    public string? Email { get; private set; }
    public bool Activo { get; private set; } = true;
    public static Cliente Crear(int empresaId, string documento, string razonSocial, string? direccion = null, string? email = null)
    {
        if (empresaId <= 0 || string.IsNullOrWhiteSpace(documento) || string.IsNullOrWhiteSpace(razonSocial)) throw new ArgumentException("Empresa, documento y razón social son obligatorios.");
        return new Cliente { EmpresaId = empresaId, Documento = documento.Trim(), RazonSocial = razonSocial.Trim(), Direccion = Texto(direccion), Email = Texto(email) };
    }
    private static string? Texto(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
