using System.Security.Claims;
using ERPContable.Application.Interfaces;
using ERPContable.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ERPContable.API.Filters;

public sealed class AuditActionFilter : IAsyncActionFilter
{
    private readonly IAuditoriaService _auditoria;
    public AuditActionFilter(IAuditoriaService auditoria) => _auditoria = auditoria;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executed = await next();
        var method = context.HttpContext.Request.Method;
        var action = context.ActionDescriptor as ControllerActionDescriptor;
        if (action?.ControllerTypeInfo.Name == "AuthController" || method is not ("POST" or "PUT" or "PATCH" or "DELETE") || context.HttpContext.Response.StatusCode >= 400)
            return;

        var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var empresa = context.HttpContext.User.FindFirstValue(AuthService.EmpresaClaim);
        await _auditoria.RegistrarAsync(
            $"{method}_{action?.ActionName}",
            action?.ControllerName ?? "API",
            userId,
            int.TryParse(empresa, out var empresaId) ? empresaId : null,
            context.HttpContext.Connection.RemoteIpAddress?.ToString(),
            context.HttpContext.Request.Path,
            context.HttpContext.RequestAborted);
    }
}
