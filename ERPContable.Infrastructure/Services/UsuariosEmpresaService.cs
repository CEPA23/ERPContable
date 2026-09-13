using ERPContable.Application.Dtos;
using ERPContable.Application.Interfaces;
using ERPContable.Domain.Entities;
using ERPContable.Persistence.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ERPContable.Infrastructure.Services;

public sealed class UsuariosEmpresaService : IUsuariosEmpresaService
{
    private static readonly HashSet<string> RolesValidos = [RolesERP.Administrador, RolesERP.Contador, RolesERP.Cajero, RolesERP.Gerente, RolesERP.Auditor];
    private readonly ERPDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public UsuariosEmpresaService(ERPDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<IReadOnlyList<UsuarioEmpresaDto>> ObtenerAsync(int empresaId, CancellationToken cancellationToken = default)
        => await _dbContext.UsuariosEmpresas.AsNoTracking().Include(x => x.Usuario)
            .Where(x => x.EmpresaId == empresaId).OrderBy(x => x.Usuario.NombreCompleto)
            .Select(x => new UsuarioEmpresaDto(x.UsuarioId, x.Usuario.NombreCompleto, x.Usuario.Email!, x.Rol, x.Activo)).ToListAsync(cancellationToken);

    public async Task AgregarAsync(int empresaId, AgregarUsuarioEmpresaDto request, CancellationToken cancellationToken = default)
    {
        var rol = ValidarRol(request.Rol);
        var user = await _userManager.FindByEmailAsync(request.CorreoElectronico.Trim()) ?? throw new ArgumentException("El usuario debe registrarse antes de asignarlo a una empresa.");
        var membership = await _dbContext.UsuariosEmpresas.FirstOrDefaultAsync(x => x.EmpresaId == empresaId && x.UsuarioId == user.Id, cancellationToken);
        if (membership is null)
        {
            _dbContext.UsuariosEmpresas.Add(new UsuarioEmpresa { EmpresaId = empresaId, UsuarioId = user.Id, Rol = rol, Activo = true, CreadoEnUtc = DateTime.UtcNow });
        }
        else
        {
            membership.Rol = rol;
            membership.Activo = true;
        }
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ActualizarAsync(int empresaId, string usuarioId, ActualizarUsuarioEmpresaDto request, CancellationToken cancellationToken = default)
    {
        var membership = await _dbContext.UsuariosEmpresas.FirstOrDefaultAsync(x => x.EmpresaId == empresaId && x.UsuarioId == usuarioId, cancellationToken)
            ?? throw new ArgumentException("El usuario no pertenece a esta empresa.");
        membership.Rol = ValidarRol(request.Rol);
        membership.Activo = request.Activo;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string ValidarRol(string rol)
    {
        var value = rol.Trim();
        if (!RolesValidos.Contains(value)) throw new ArgumentException("Rol no válido.");
        return value;
    }
}
