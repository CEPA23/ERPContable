using ERPContable.Application.Dtos;
using ERPContable.Application.Interfaces;
using ERPContable.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPContable.API.Controllers;

[ApiController]
[Authorize(Policy = ErpPolicies.Usuarios)]
[Route("api/empresas/{empresaId:int}/usuarios")]
public sealed class UsuariosEmpresaController : ControllerBase
{
    private readonly IUsuariosEmpresaService _service;
    public UsuariosEmpresaController(IUsuariosEmpresaService service) => _service = service;

    [HttpGet]
    public Task<IReadOnlyList<UsuarioEmpresaDto>> Obtener(int empresaId, CancellationToken ct) => _service.ObtenerAsync(empresaId, ct);

    [HttpPost]
    public async Task<IActionResult> Agregar(int empresaId, AgregarUsuarioEmpresaDto request, CancellationToken ct)
    {
        try { await _service.AgregarAsync(empresaId, request, ct); return NoContent(); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPut("{usuarioId}")]
    public async Task<IActionResult> Actualizar(int empresaId, string usuarioId, ActualizarUsuarioEmpresaDto request, CancellationToken ct)
    {
        try { await _service.ActualizarAsync(empresaId, usuarioId, request, ct); return NoContent(); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }
}
