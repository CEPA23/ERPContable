using System.Security.Claims;
using ERPContable.Application.Dtos;
using ERPContable.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPContable.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class EmpresasController : ControllerBase
{
    private readonly IEmpresaAccesoService _empresaService;
    public EmpresasController(IEmpresaAccesoService empresaService) => _empresaService = empresaService;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EmpresaDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _empresaService.ObtenerParaUsuarioAsync(UserId(), cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmpresaDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var empresa = await _empresaService.ObtenerParaUsuarioAsync(UserId(), id, cancellationToken);
        return empresa is null ? NotFound() : Ok(empresa);
    }

    [HttpPost]
    public async Task<ActionResult<EmpresaDto>> Create([FromBody] EmpresaCreateDto request, CancellationToken cancellationToken)
    {
        var created = await _empresaService.CrearParaUsuarioAsync(UserId(), request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] EmpresaUpdateDto request, CancellationToken cancellationToken)
    {
        var updated = await _empresaService.ActualizarParaUsuarioAsync(UserId(), id, request, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _empresaService.EliminarParaUsuarioAsync(UserId(), id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    private string UserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
}
