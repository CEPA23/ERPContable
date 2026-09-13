using ERPContable.Application.Dtos;
using ERPContable.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ERPContable.API.Controllers;

[ApiController]
[Authorize]
[Route("api/empresas/{empresaId:int}/catalogo")]
public sealed class CatalogoController : ControllerBase
{
    private readonly ICatalogoService _service;
    public CatalogoController(ICatalogoService service) => _service = service;

    [HttpGet]
    public Task<CatalogoDto> Obtener(int empresaId, CancellationToken ct) => _service.ObtenerAsync(empresaId, ct);

    [HttpPost("categorias")]
    public async Task<ActionResult<CategoriaDto>> Categoria(int empresaId, CategoriaCreateDto request, CancellationToken ct) => await Execute(() => _service.CrearCategoriaAsync(empresaId, request, ct));

    [HttpPost("unidades")]
    public async Task<ActionResult<UnidadMedidaDto>> Unidad(int empresaId, UnidadMedidaCreateDto request, CancellationToken ct) => await Execute(() => _service.CrearUnidadAsync(empresaId, request, ct));

    [HttpPost("impuestos")]
    public async Task<ActionResult<ImpuestoDto>> Impuesto(int empresaId, ImpuestoCreateDto request, CancellationToken ct) => await Execute(() => _service.CrearImpuestoAsync(empresaId, request, ct));

    [HttpPost("productos")]
    public async Task<ActionResult<ProductoDto>> Producto(int empresaId, ProductoCreateDto request, CancellationToken ct) => await Execute(() => _service.CrearProductoAsync(empresaId, request, ct));

    [HttpPost("centros-costo")]
    public async Task<ActionResult<CentroCostoDto>> CentroCosto(int empresaId, CentroCostoCreateDto request, CancellationToken ct) => await Execute(() => _service.CrearCentroCostoAsync(empresaId, request, ct));

    private static async Task<ActionResult<T>> Execute<T>(Func<Task<T>> action)
    {
        try { return new ActionResult<T>(await action()); }
        catch (ArgumentException ex) { return new BadRequestObjectResult(new { message = ex.Message }); }
    }
}
