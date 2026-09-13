using ERPContable.Application.Dtos;
using ERPContable.Application.Interfaces;
using ERPContable.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace ERPContable.API.Controllers;
[ApiController]
[Authorize(Policy = ErpPolicies.Activos)]
[Route("api/empresas/{empresaId:int}/activos")]
public sealed class ActivosController : ControllerBase
{
    private readonly IActivosService _service; public ActivosController(IActivosService service) => _service = service;
    [HttpGet] public Task<IReadOnlyList<ActivoDto>> Obtener(int empresaId, CancellationToken ct) => _service.ObtenerAsync(empresaId, ct);
    [HttpPost] public async Task<ActionResult<ActivoDto>> Crear(int empresaId, ActivoCreateDto request, CancellationToken ct) { try { return Ok(await _service.CrearAsync(empresaId, request, ct)); } catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); } }
    [HttpPost("{activoId:int}/depreciar")] public async Task<ActionResult<ActivoDto>> Depreciar(int empresaId, int activoId, DepreciarActivoDto request, CancellationToken ct) { try { return Ok(await _service.DepreciarAsync(empresaId, activoId, request, ct)); } catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); } }
}
