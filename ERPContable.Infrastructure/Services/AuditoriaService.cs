using ERPContable.Application.Interfaces;
using ERPContable.Domain.Entities;
using ERPContable.Persistence.Data;

namespace ERPContable.Infrastructure.Services;

public sealed class AuditoriaService : IAuditoriaService
{
    private readonly ERPDbContext _dbContext;
    public AuditoriaService(ERPDbContext dbContext) => _dbContext = dbContext;

    public async Task RegistrarAsync(string accion, string entidad, string? usuarioId, int? empresaId, string? direccionIp, string? detalles = null, CancellationToken cancellationToken = default)
    {
        _dbContext.Auditoria.Add(new AuditLog
        {
            Accion = accion,
            Entidad = entidad,
            UsuarioId = usuarioId,
            EmpresaId = empresaId,
            DireccionIp = direccionIp,
            Detalles = detalles,
            CreadoEnUtc = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
