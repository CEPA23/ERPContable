using ERPContable.Application.Dtos;
using ERPContable.Application.Interfaces;
using ERPContable.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPContable.API.Controllers;

[ApiController]
[Authorize(Policy = ErpPolicies.Configuracion)]
[Route("api/empresas/{empresaId:int}/configuracion-contable")]
public sealed class ConfiguracionContableController : ControllerBase
{
    private readonly IConfiguracionContableService _service;
    public ConfiguracionContableController(IConfiguracionContableService service) => _service = service;

    [HttpGet]
    public Task<ConfiguracionContableDto> Obtener(int empresaId, CancellationToken ct) => _service.ObtenerAsync(empresaId, ct);

    [HttpPut]
    public async Task<ActionResult<ConfiguracionContableDto>> Actualizar(int empresaId, ActualizarConfiguracionContableDto request, CancellationToken ct)
    {
        try { return Ok(await _service.ActualizarAsync(empresaId, request, ct)); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }
}
