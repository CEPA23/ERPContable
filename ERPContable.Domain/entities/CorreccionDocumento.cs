namespace ERPContable.Domain.Entities;

// Append-only history: original documents and journal entries are never deleted.
public sealed class CorreccionDocumento
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public int? CompraId { get; set; }
    public int? VentaId { get; set; }
    public string Tipo { get; set; } = "DEVOLUCION";
    public string Motivo { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public DateTime CreadoEnUtc { get; set; } = DateTime.UtcNow;
    public string UsuarioId { get; set; } = string.Empty;
    public string UsuarioNombre { get; set; } = string.Empty;
    public Guid SolicitudId { get; set; }
    public bool Completa { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Igv { get; set; }
    public int? AsientoContableId { get; set; }
    public AsientoContable? AsientoContable { get; set; }
    public int? AsientoCostoId { get; set; }
    public AsientoContable? AsientoCosto { get; set; }
    public List<CorreccionDetalle> Detalles { get; set; } = [];
}

public sealed class CorreccionDetalle
{
    public int Id { get; set; }
    public int CorreccionDocumentoId { get; set; }
    public int? CompraDetalleId { get; set; }
    public int? VentaDetalleId { get; set; }
    public decimal Cantidad { get; set; }
    public decimal Subtotal { get; set; }
    public decimal CostoInventario { get; set; }
    public int? MovimientoInventarioId { get; set; }
    public MovimientoInventario? MovimientoInventario { get; set; }
}
