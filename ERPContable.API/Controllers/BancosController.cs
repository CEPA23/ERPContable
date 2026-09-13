using ERPContable.Application.Dtos;
using ERPContable.Application.Interfaces;
using ERPContable.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPContable.API.Controllers;

[ApiController]
[Authorize(Policy = ErpPolicies.Bancos)]
[Route("api/empresas/{empresaId:int}/bancos")]
public sealed class BancosController : ControllerBase
{
    private readonly IBancosService _service;
    public BancosController(IBancosService service) => _service = service;
    [HttpGet("cuentas")] public Task<IReadOnlyList<CuentaBancariaDto>> Cuentas(int empresaId, CancellationToken ct) => _service.ObtenerCuentasAsync(empresaId, ct);
    [HttpPost("cuentas")] public async Task<ActionResult<CuentaBancariaDto>> CrearCuenta(int empresaId, CuentaBancariaCreateDto request, CancellationToken ct) { try { return Ok(await _service.CrearCuentaAsync(empresaId, request, ct)); } catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); } }
    [HttpGet] public Task<IReadOnlyList<MovimientoBancoDto>> Movimientos(int empresaId, CancellationToken ct) => _service.ObtenerMovimientosAsync(empresaId, ct);
    [HttpPost] public async Task<ActionResult<MovimientoBancoDto>> CrearMovimiento(int empresaId, MovimientoBancoCreateDto request, CancellationToken ct) { try { return Ok(await _service.CrearMovimientoAsync(empresaId, request, ct)); } catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); } }
}
