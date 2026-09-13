using ERPContable.Application.Dtos;
using ERPContable.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPContable.API.Controllers;

[ApiController]
[Authorize]
[Route("api/empresas/{empresaId:int}/inventario")]
public sealed class InventarioController : ControllerBase
{
    private readonly IInventarioService _service;
    public InventarioController(IInventarioService service) => _service = service;

    [HttpGet]
    public Task<InventarioDto> Obtener(int empresaId, [FromQuery] int? productoId, CancellationToken ct) => _service.ObtenerAsync(empresaId, productoId, ct);

    [HttpPost("movimientos")]
    public async Task<ActionResult<MovimientoInventarioDto>> Movimiento(int empresaId, MovimientoInventarioCreateDto request, CancellationToken ct) => await Execute(() => _service.RegistrarMovimientoAsync(empresaId, request, ct));

    [HttpPut("productos/{productoId:int}/stock-minimo")]
    public async Task<ActionResult<InventarioProductoDto>> StockMinimo(int empresaId, int productoId, StockMinimoUpdateDto request, CancellationToken ct) => await Execute(() => _service.ActualizarStockMinimoAsync(empresaId, productoId, request, ct));

    private static async Task<ActionResult<T>> Execute<T>(Func<Task<T>> action)
    {
        try { return new ActionResult<T>(await action()); }
        catch (ArgumentException ex) { return new BadRequestObjectResult(new { message = ex.Message }); }
    }
}
