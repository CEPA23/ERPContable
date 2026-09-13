using ERPContable.Application.Dtos;
using ERPContable.Application.Interfaces;
using ERPContable.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPContable.API.Controllers;

[ApiController]
[Authorize(Policy = ErpPolicies.Compras)]
[Route("api/empresas/{empresaId:int}/compras")]
public sealed class ComprasController : ControllerBase
{
    private readonly IComprasService _service;
    public ComprasController(IComprasService service) => _service = service;

    [HttpGet("proveedores")]
    public Task<IReadOnlyList<ProveedorDto>> Proveedores(int empresaId, CancellationToken ct) => _service.ObtenerProveedoresAsync(empresaId, ct);

    [HttpPost("proveedores")]
    public async Task<ActionResult<ProveedorDto>> CrearProveedor(int empresaId, ProveedorCreateDto request, CancellationToken ct)
    {
        try { return Ok(await _service.CrearProveedorAsync(empresaId, request, ct)); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet]
    public Task<IReadOnlyList<CompraDto>> Compras(int empresaId, CancellationToken ct) => _service.ObtenerComprasAsync(empresaId, ct);

    [HttpPost]
    public async Task<ActionResult<CompraDto>> Crear(int empresaId, CompraCreateDto request, CancellationToken ct)
    {
        try { return Ok(await _service.CrearCompraAsync(empresaId, request, ct)); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }
}
