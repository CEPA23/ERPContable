namespace ERPContable.Application.Interfaces;

public interface IAuditoriaService
{
    Task RegistrarAsync(string accion, string entidad, string? usuarioId, int? empresaId, string? direccionIp, string? detalles = null, CancellationToken cancellationToken = default);
}
