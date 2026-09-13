using System.Security.Claims;
using ERPContable.Application.Dtos;
using ERPContable.Application.Security;
using ERPContable.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ERPContable.API.Controllers;

[ApiController]
[Route("api/empresas/{empresaId:int}")]
public sealed class CorreccionesController(CorreccionesService service) : ControllerBase
{
    [HttpGet("compras/{id:int}/correcciones")]
    [Authorize(Policy = ErpPolicies.Compras)]
    public Task<ActionResult<CorreccionesDocumentoDto>> ObtenerCompra(int empresaId, int id, CancellationToken ct)
        => Ejecutar(() => service.ObtenerAsync(empresaId, false, id, ct));

    [HttpGet("ventas/{id:int}/correcciones")]
    [Authorize(Policy = ErpPolicies.Ventas)]
    public Task<ActionResult<CorreccionesDocumentoDto>> ObtenerVenta(int empresaId, int id, CancellationToken ct)
        => Ejecutar(() => service.ObtenerAsync(empresaId, true, id, ct));

    [HttpPost("compras/{id:int}/correcciones")]
    [Authorize(Policy = ErpPolicies.Compras)]
    public Task<ActionResult<CorreccionesDocumentoDto>> CorregirCompra(int empresaId, int id, CorreccionRequest request, CancellationToken ct)
        => Registrar(empresaId, false, id, request, ct);

    [HttpPost("ventas/{id:int}/correcciones")]
    [Authorize(Policy = ErpPolicies.Ventas)]
    public Task<ActionResult<CorreccionesDocumentoDto>> CorregirVenta(int empresaId, int id, CorreccionRequest request, CancellationToken ct)
        => Registrar(empresaId, true, id, request, ct);

    private Task<ActionResult<CorreccionesDocumentoDto>> Registrar(int empresaId, bool venta, int id, CorreccionRequest request, CancellationToken ct)
        => Ejecutar(() => service.RegistrarAsync(empresaId, venta, id, request,
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException(), User.Identity?.Name ?? "Usuario", ct));

    private async Task<ActionResult<CorreccionesDocumentoDto>> Ejecutar(Func<Task<CorreccionesDocumentoDto>> action)
    {
        try { return Ok(await action()); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (DbUpdateConcurrencyException) { return Conflict(new { message = "El inventario cambió mientras guardabas. Actualiza el documento y vuelve a intentar." }); }
        catch (Exception ex) when (EsConflictoDePersistencia(ex))
        { return Conflict(new { message = "Otra operación se registró al mismo tiempo. Actualiza el documento antes de volver a intentar." }); }
    }

    private static bool EsConflictoDePersistencia(Exception ex)
        => ex is PostgresException { SqlState: "40001" or "40P01" or "23505" }
           || ex.InnerException is PostgresException { SqlState: "40001" or "40P01" or "23505" }
           || ex is Microsoft.Data.Sqlite.SqliteException { SqliteErrorCode: 5 or 6 or 19 }
           || ex.InnerException is Microsoft.Data.Sqlite.SqliteException { SqliteErrorCode: 5 or 6 or 19 };
}
