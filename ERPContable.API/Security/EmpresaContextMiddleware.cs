using System.Security.Claims;
using ERPContable.Domain.Entities;
using ERPContable.Infrastructure.Services;
using ERPContable.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPContable.API.Security;

public sealed class EmpresaContextMiddleware
{
    private readonly RequestDelegate _next;
    public EmpresaContextMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ERPDbContext dbContext)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        if (!int.TryParse(context.Request.RouteValues["empresaId"]?.ToString(), out var requestedEmpresaId))
        {
            await _next(context);
            return;
        }

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var selectedEmpresa = context.User.FindFirstValue(AuthService.EmpresaClaim);
        if (string.IsNullOrWhiteSpace(userId) || selectedEmpresa != requestedEmpresaId.ToString())
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        var rolActual = await dbContext.UsuariosEmpresas.AsNoTracking().Include(x => x.Empresa)
            .Where(x => x.UsuarioId == userId && x.EmpresaId == requestedEmpresaId && x.Activo && x.Empresa.Activa)
            .Select(x => x.Rol).FirstOrDefaultAsync(context.RequestAborted);
        if (rolActual is null || context.User.FindFirstValue(ClaimTypes.Role) != rolActual)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        await _next(context);
    }
}
