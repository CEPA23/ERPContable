using ERPContable.Application.Dtos;
using ERPContable.Application.Interfaces;
using ERPContable.Domain.Entities;
using ERPContable.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPContable.Infrastructure.Services;

public sealed class EmpresaAccesoService : IEmpresaAccesoService
{
    private readonly ERPDbContext _dbContext;
    private readonly IContabilidadService _contabilidad;
    public EmpresaAccesoService(ERPDbContext dbContext, IContabilidadService contabilidad) { _dbContext = dbContext; _contabilidad = contabilidad; }

    public async Task<IReadOnlyList<EmpresaDto>> ObtenerParaUsuarioAsync(string usuarioId, CancellationToken cancellationToken = default)
        => await _dbContext.UsuariosEmpresas.AsNoTracking().Include(x => x.Empresa)
            .Where(x => x.UsuarioId == usuarioId && x.Activo && x.Empresa.Activa).OrderBy(x => x.Empresa.RazonSocial)
            .Select(x => new EmpresaDto(x.Empresa.Id, x.Empresa.RazonSocial, x.Empresa.DocumentoIdentidad, x.Empresa.NombreComercial, x.Empresa.Direccion, x.Empresa.Telefono, x.Empresa.Email, x.Empresa.Activa, x.Empresa.CreadaEnUtc, x.Empresa.ActualizadaEnUtc, x.Empresa.EstadoSunat, x.Empresa.CondicionSunat)).ToListAsync(cancellationToken);

    public async Task<EmpresaDto?> ObtenerParaUsuarioAsync(string usuarioId, int empresaId, CancellationToken cancellationToken = default)
    {
        var membership = await _dbContext.UsuariosEmpresas.AsNoTracking().Include(x => x.Empresa)
            .FirstOrDefaultAsync(x => x.UsuarioId == usuarioId && x.EmpresaId == empresaId && x.Activo, cancellationToken);
        return membership is null ? null : Map(membership.Empresa);
    }

    public async Task<EmpresaDto> CrearParaUsuarioAsync(string usuarioId, EmpresaCreateDto request, CancellationToken cancellationToken = default)
    {
        var empresa = Empresa.Crear(request.RazonSocial, request.DocumentoIdentidad, request.NombreComercial, request.Direccion, request.Telefono, request.Email, request.EstadoSunat, request.CondicionSunat);
        _dbContext.Empresas.Add(empresa);
        _dbContext.UsuariosEmpresas.Add(new UsuarioEmpresa { UsuarioId = usuarioId, Empresa = empresa, Rol = RolesERP.Administrador, Activo = true, CreadoEnUtc = DateTime.UtcNow });
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _contabilidad.InicializarPlanContableAsync(empresa.Id, cancellationToken);
        return Map(empresa);
    }

    public async Task<bool> ActualizarParaUsuarioAsync(string usuarioId, int empresaId, EmpresaUpdateDto request, CancellationToken cancellationToken = default)
    {
        var membership = await AdministradorAsync(usuarioId, empresaId, cancellationToken);
        if (membership is null) return false;
        membership.Empresa.Actualizar(request.RazonSocial, request.DocumentoIdentidad, request.NombreComercial, request.Direccion, request.Telefono, request.Email, request.Activa, request.EstadoSunat, request.CondicionSunat);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> EliminarParaUsuarioAsync(string usuarioId, int empresaId, CancellationToken cancellationToken = default)
    {
        var membership = await AdministradorAsync(usuarioId, empresaId, cancellationToken);
        if (membership is null) return false;
        _dbContext.Empresas.Remove(membership.Empresa);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private Task<UsuarioEmpresa?> AdministradorAsync(string usuarioId, int empresaId, CancellationToken cancellationToken)
        => _dbContext.UsuariosEmpresas.Include(x => x.Empresa).FirstOrDefaultAsync(x => x.UsuarioId == usuarioId && x.EmpresaId == empresaId && x.Activo && x.Rol == RolesERP.Administrador, cancellationToken);

    private static EmpresaDto Map(Empresa empresa) => new(empresa.Id, empresa.RazonSocial, empresa.DocumentoIdentidad, empresa.NombreComercial, empresa.Direccion, empresa.Telefono, empresa.Email, empresa.Activa, empresa.CreadaEnUtc, empresa.ActualizadaEnUtc, empresa.EstadoSunat, empresa.CondicionSunat);
}
