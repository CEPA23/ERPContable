namespace ERPContable.Domain.Entities;

public class AsientoContable
{
    private AsientoContable() { }

    public int Id { get; private set; }
    public int EmpresaId { get; private set; }
    public Empresa? Empresa { get; private set; }
    public DateTime Fecha { get; private set; }
    public string Glosa { get; private set; } = string.Empty;
    public DateTime CreadoEnUtc { get; private set; }
    public List<DetalleAsiento> Detalles { get; private set; } = [];

    public static AsientoContable Crear(int empresaId, DateTime fecha, string glosa, IEnumerable<(int cuentaId, decimal debe, decimal haber)> detalles)
    {
        if (empresaId <= 0) throw new ArgumentException("La empresa es obligatoria.");
        var items = detalles.ToList();
        if (items.Count < 2 || items.Sum(x => x.debe) != items.Sum(x => x.haber))
            throw new ArgumentException("El asiento debe tener al menos dos líneas y el debe debe ser igual al haber.");
        if (items.Any(x => x.debe < 0 || x.haber < 0 || (x.debe > 0 && x.haber > 0) || (x.debe == 0 && x.haber == 0)))
            throw new ArgumentException("Cada línea debe tener un importe en debe o en haber, no en ambos.");

        var asiento = new AsientoContable { EmpresaId = empresaId, Fecha = fecha.Date, Glosa = glosa.Trim(), CreadoEnUtc = DateTime.UtcNow };
        asiento.Detalles = items.Select(x => DetalleAsiento.Crear(x.cuentaId, x.debe, x.haber)).ToList();
        return asiento;
    }
}

public class DetalleAsiento
{
    private DetalleAsiento() { }
    public int Id { get; private set; }
    public int AsientoContableId { get; private set; }
    public int CuentaContableId { get; private set; }
    public decimal Debe { get; private set; }
    public decimal Haber { get; private set; }
    public CuentaContable? CuentaContable { get; private set; }

    public static DetalleAsiento Crear(int cuentaId, decimal debe, decimal haber)
        => new() { CuentaContableId = cuentaId, Debe = debe, Haber = haber };
}
