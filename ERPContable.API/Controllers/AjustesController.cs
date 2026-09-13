using ERPContable.Application.Dtos;
using ERPContable.Application.Interfaces;
using ERPContable.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace ERPContable.API.Controllers;
[ApiController]
[Authorize(Policy = ErpPolicies.Ajustes)]
[Route("api/empresas/{empresaId:int}/ajustes")]
public sealed class AjustesController : ControllerBase
{
    private readonly IAjustesService _service; public AjustesController(IAjustesService service) => _service = service;
    [HttpGet] public Task<IReadOnlyList<AjusteDto>> Obtener(int empresaId, CancellationToken ct) => _service.ObtenerAsync(empresaId, ct);
    [HttpPost] public async Task<ActionResult<AjusteDto>> Crear(int empresaId, AjusteCreateDto request, CancellationToken ct) { try { return Ok(await _service.CrearAsync(empresaId, request, ct)); } catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); } }
}
