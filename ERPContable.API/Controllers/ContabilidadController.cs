using ERPContable.Application.Dtos;
using ERPContable.Application.Interfaces;
using ERPContable.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPContable.API.Controllers;

[ApiController]
[Authorize(Policy = ErpPolicies.Contabilidad)]
[Route("api/empresas/{empresaId:int}/contabilidad")]
public sealed class ContabilidadController : ControllerBase
{
    private readonly IContabilidadService _service;
    public ContabilidadController(IContabilidadService service) => _service = service;

    [HttpGet("cuentas")]
    public async Task<ActionResult<IReadOnlyList<CuentaContableDto>>> Cuentas(int empresaId, CancellationToken ct) => Ok(await _service.GetCuentasAsync(empresaId, ct));

    [HttpPost("cuentas")]
    [Authorize(Policy = ErpPolicies.Configuracion)]
    public async Task<ActionResult<CuentaContableDto>> CrearCuenta(int empresaId, CuentaContableCreateDto request, CancellationToken ct)
    {
        try { return Ok(await _service.CrearCuentaAsync(empresaId, request, ct)); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPatch("cuentas/{cuentaId:int}/estado")]
    [Authorize(Policy = ErpPolicies.Configuracion)]
    public async Task<ActionResult<CuentaContableDto>> CambiarEstadoCuenta(int empresaId, int cuentaId, CambiarEstadoCuentaDto request, CancellationToken ct)
    {
        try { return Ok(await _service.CambiarEstadoCuentaAsync(empresaId, cuentaId, request, ct)); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet("asientos")]
    public async Task<ActionResult<IReadOnlyList<AsientoDto>>> Asientos(int empresaId, CancellationToken ct) => Ok(await _service.GetAsientosAsync(empresaId, ct));

    [HttpPost("asientos")]
    public async Task<ActionResult<AsientoDto>> CrearAsiento(int empresaId, AsientoCreateDto request, CancellationToken ct)
    {
        try { return Ok(await _service.CrearAsientoAsync(empresaId, request, ct)); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }
}
