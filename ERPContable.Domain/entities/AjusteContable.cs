namespace ERPContable.Domain.Entities;

public class AjusteContable
{
    private AjusteContable() { }
    public int Id { get; private set; }
    public int EmpresaId { get; private set; }
    public DateTime Fecha { get; private set; }
    public string Glosa { get; private set; } = string.Empty;
    public int AsientoContableId { get; private set; }
    public static AjusteContable Crear(int empresaId, DateTime fecha, string glosa, int asientoId)
    {
        if (empresaId <= 0 || string.IsNullOrWhiteSpace(glosa) || asientoId <= 0) throw new ArgumentException("Los datos del ajuste son obligatorios.");
        return new AjusteContable { EmpresaId = empresaId, Fecha = fecha.Date, Glosa = glosa.Trim(), AsientoContableId = asientoId };
    }
}
