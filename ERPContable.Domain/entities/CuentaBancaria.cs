namespace ERPContable.Domain.Entities;

public class CuentaBancaria
{
    private CuentaBancaria() { }
    public int Id { get; private set; }
    public int EmpresaId { get; private set; }
    public Empresa? Empresa { get; private set; }
    public string Banco { get; private set; } = string.Empty;
    public string NumeroCuenta { get; private set; } = string.Empty;
    public string Moneda { get; private set; } = "PEN";
    public int CuentaContableId { get; private set; }
    public CuentaContable? CuentaContable { get; private set; }
    public bool Activa { get; private set; } = true;

    public static CuentaBancaria Crear(int empresaId, string banco, string numeroCuenta, string moneda, int cuentaContableId)
    {
        if (empresaId <= 0 || string.IsNullOrWhiteSpace(banco) || string.IsNullOrWhiteSpace(numeroCuenta) || cuentaContableId <= 0)
            throw new ArgumentException("Empresa, banco, número de cuenta y cuenta contable son obligatorios.");
        return new CuentaBancaria { EmpresaId = empresaId, Banco = banco.Trim(), NumeroCuenta = numeroCuenta.Trim(), Moneda = string.IsNullOrWhiteSpace(moneda) ? "PEN" : moneda.Trim().ToUpperInvariant(), CuentaContableId = cuentaContableId };
    }
}
