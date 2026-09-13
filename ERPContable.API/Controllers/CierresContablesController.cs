using System.Security.Claims;
using ERPContable.Application.Dtos;
using ERPContable.Application.Interfaces;
using ERPContable.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPContable.API.Controllers;

[ApiController]
[Authorize(Policy = ErpPolicies.Configuracion)]
[Route("api/empresas/{empresaId:int}/cierres")]
public sealed class CierresContablesController : ControllerBase
{
    private readonly ICierreContableService _service;
    public CierresContablesController(ICierreContableService service) => _service = service;

    [HttpGet]
    public Task<EstadoCierresDto> Estado(int empresaId, [FromQuery] int ejercicio, CancellationToken ct) => _service.ObtenerEstadoAsync(empresaId, ejercicio, ct);

    [HttpPost("periodos/{mes:int}/cerrar")]
    public async Task<ActionResult<CierrePeriodoDto>> CerrarPeriodo(int empresaId, int mes, [FromQuery] int ejercicio, CerrarPeriodoDto request, CancellationToken ct)
    {
        try { return Ok(await _service.CerrarPeriodoAsync(empresaId, ejercicio, mes, UserId(), request, ct)); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("periodos/{mes:int}/reabrir")]
    public async Task<ActionResult<CierrePeriodoDto>> ReabrirPeriodo(int empresaId, int mes, [FromQuery] int ejercicio, CerrarPeriodoDto request, CancellationToken ct)
    {
        try { return Ok(await _service.ReabrirPeriodoAsync(empresaId, ejercicio, mes, UserId(), request, ct)); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("ejercicio/cerrar")]
    public async Task<ActionResult<CierreEjercicioDto>> CerrarEjercicio(int empresaId, [FromQuery] int ejercicio, CerrarEjercicioDto request, CancellationToken ct)
    {
        try { return Ok(await _service.CerrarEjercicioAsync(empresaId, ejercicio, UserId(), request, ct)); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("ejercicio/reabrir")]
    public async Task<ActionResult<CierreEjercicioDto>> ReabrirEjercicio(int empresaId, [FromQuery] int ejercicio, CerrarEjercicioDto request, CancellationToken ct)
    {
        try { return Ok(await _service.ReabrirEjercicioAsync(empresaId, ejercicio, UserId(), request, ct)); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    private string UserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
}
