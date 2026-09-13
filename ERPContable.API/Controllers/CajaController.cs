using ERPContable.Application.Dtos;
using ERPContable.Application.Interfaces;
using ERPContable.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPContable.API.Controllers;

[ApiController]
[Authorize(Policy = ErpPolicies.Caja)]
[Route("api/empresas/{empresaId:int}/caja")]
public sealed class CajaController : ControllerBase
{
    private readonly ICajaService _service;
    public CajaController(ICajaService service) => _service = service;
    [HttpGet] public Task<IReadOnlyList<MovimientoCajaDto>> Movimientos(int empresaId, CancellationToken ct) => _service.ObtenerMovimientosAsync(empresaId, ct);
    [HttpPost] public async Task<ActionResult<MovimientoCajaDto>> Crear(int empresaId, MovimientoCajaCreateDto request, CancellationToken ct) { try { return Ok(await _service.CrearMovimientoAsync(empresaId, request, ct)); } catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); } }
}
