using ERPContable.Application.Dtos;
using ERPContable.Application.Interfaces;
using ERPContable.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace ERPContable.API.Controllers;
[ApiController]
[Authorize(Policy = ErpPolicies.Ventas)]
[Route("api/empresas/{empresaId:int}/ventas")]
public sealed class VentasController : ControllerBase
{
    private readonly IVentasService _service; public VentasController(IVentasService service) => _service = service;
    [HttpGet("clientes")] public Task<IReadOnlyList<ClienteDto>> Clientes(int empresaId, CancellationToken ct) => _service.ObtenerClientesAsync(empresaId, ct);
    [HttpPost("clientes")] public async Task<ActionResult<ClienteDto>> CrearCliente(int empresaId, ClienteCreateDto request, CancellationToken ct) { try { return Ok(await _service.CrearClienteAsync(empresaId, request, ct)); } catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); } }
    [HttpGet] public Task<IReadOnlyList<VentaDto>> Ventas(int empresaId, CancellationToken ct) => _service.ObtenerVentasAsync(empresaId, ct);
    [HttpPost] public async Task<ActionResult<VentaDto>> Crear(int empresaId, VentaCreateDto request, CancellationToken ct) { try { return Ok(await _service.CrearVentaAsync(empresaId, request, ct)); } catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); } }
}
