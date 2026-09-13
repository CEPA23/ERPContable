namespace ERPContable.Domain.Entities;

public sealed class CuentaContable
{
    private static readonly HashSet<string> TiposPermitidos = ["ACTIVO", "PASIVO", "PATRIMONIO", "INGRESO", "GASTO"];
    private CuentaContable() { }

    public int Id { get; private set; }
    public int? EmpresaId { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public string Tipo { get; private set; } = string.Empty;
    public bool Activa { get; private set; } = true;

    // Las plantillas no se usan en asientos: solo se copian al crear una empresa.
    public static CuentaContable Crear(string codigo, string nombre, string tipo)
        => CrearInterno(null, codigo, nombre, tipo);

    public static CuentaContable CrearParaEmpresa(int empresaId, string codigo, string nombre, string tipo)
    {
        if (empresaId <= 0) throw new ArgumentException("La empresa es obligatoria.");
        return CrearInterno(empresaId, codigo, nombre, tipo);
    }

    public static CuentaContable CopiarParaEmpresa(int empresaId, CuentaContable plantilla)
        => CrearParaEmpresa(empresaId, plantilla.Codigo, plantilla.Nombre, plantilla.Tipo);

    public void CambiarEstado(bool activa) => Activa = activa;

    private static CuentaContable CrearInterno(int? empresaId, string codigo, string nombre, string tipo)
    {
        var codigoNormalizado = codigo?.Trim() ?? string.Empty;
        var nombreNormalizado = nombre?.Trim() ?? string.Empty;
        var tipoNormalizado = tipo?.Trim().ToUpperInvariant() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(codigoNormalizado) || codigoNormalizado.Length > 20) throw new ArgumentException("El código contable es obligatorio y no puede superar 20 caracteres.");
        if (string.IsNullOrWhiteSpace(nombreNormalizado) || nombreNormalizado.Length > 200) throw new ArgumentException("El nombre de la cuenta es obligatorio y no puede superar 200 caracteres.");
        if (!TiposPermitidos.Contains(tipoNormalizado)) throw new ArgumentException("El tipo de cuenta no es válido.");
        return new CuentaContable { EmpresaId = empresaId, Codigo = codigoNormalizado, Nombre = nombreNormalizado, Tipo = tipoNormalizado, Activa = true };
    }
}
