using ERPContable.Application.Dtos;
using ERPContable.Application.Interfaces;
using ERPContable.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPContable.API.Controllers;

[ApiController]
[Authorize(Policy = ErpPolicies.Dashboard)]
[Route("api/empresas/{empresaId:int}/dashboard")]
public sealed class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;
    public DashboardController(IDashboardService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<DashboardResumenDto>> Obtener(int empresaId, [FromQuery] int ejercicio, [FromQuery] int mes, CancellationToken ct)
    {
        try { return Ok(await _service.ObtenerResumenAsync(empresaId, ejercicio, mes, ct)); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }
}
