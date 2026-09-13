using ERPContable.Application.Dtos;
using ERPContable.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPContable.API.Controllers;

[ApiController]
[Authorize]
[Route("api/consultas")]
public sealed class ConsultasController : ControllerBase
{
    private readonly IApisPeruService _apisPeruService;

    public ConsultasController(IApisPeruService apisPeruService)
    {
        _apisPeruService = apisPeruService;
    }

    [HttpGet("ruc/{ruc}")]
    public Task<ActionResult<ConsultaDocumentoDto?>> ConsultarRuc(string ruc, CancellationToken cancellationToken)
        => ConsultarAsync(ruc, esRuc: true, cancellationToken);

    [HttpGet("dni/{dni}")]
    public Task<ActionResult<ConsultaDocumentoDto?>> ConsultarDni(string dni, CancellationToken cancellationToken)
        => ConsultarAsync(dni, esRuc: false, cancellationToken);

    private async Task<ActionResult<ConsultaDocumentoDto?>> ConsultarAsync(
        string documento,
        bool esRuc,
        CancellationToken cancellationToken)
    {
        var esperado = esRuc ? 11 : 8;
        if (documento.Length != esperado || documento.Any(char.IsLetter))
        {
            return BadRequest($"El {(esRuc ? "RUC" : "DNI")} debe tener {esperado} dígitos.");
        }

        try
        {
            var result = esRuc
                ? await _apisPeruService.ConsultarRucAsync(documento, cancellationToken)
                : await _apisPeruService.ConsultarDniAsync(documento, cancellationToken);

            return result is null ? NotFound("No se encontró información para ese documento.") : Ok(result);
        }
        catch (InvalidOperationException exception)
        {
            return Problem(exception.Message, statusCode: StatusCodes.Status503ServiceUnavailable);
        }
        catch (HttpRequestException exception)
        {
            return Problem(exception.Message, statusCode: StatusCodes.Status502BadGateway);
        }
    }
}
