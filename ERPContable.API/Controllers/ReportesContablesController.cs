using ERPContable.Application.Dtos;
using ERPContable.Application.Interfaces;
using ERPContable.API.Services;
using ERPContable.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPContable.API.Controllers;

[ApiController]
[Authorize(Policy = ErpPolicies.Reportes)]
[Route("api/empresas/{empresaId:int}/reportes")]
public sealed class ReportesContablesController : ControllerBase
{
    private readonly IReportesContablesService _service;
    private readonly ReportesExportService _exportService;

    public ReportesContablesController(IReportesContablesService service, ReportesExportService exportService)
    {
        _service = service;
        _exportService = exportService;
    }

    [HttpGet("libro-diario")]
    public Task<LibroDiarioDto> LibroDiario(int empresaId, [FromQuery] DateTime desde, [FromQuery] DateTime hasta, CancellationToken ct)
        => _service.ObtenerLibroDiarioAsync(empresaId, desde, hasta, ct);

    [HttpGet("libro-mayor")]
    public Task<IReadOnlyList<LibroMayorCuentaDto>> LibroMayor(int empresaId, [FromQuery] DateTime desde, [FromQuery] DateTime hasta, CancellationToken ct)
        => _service.ObtenerLibroMayorAsync(empresaId, desde, hasta, ct);

    [HttpGet("balance-comprobacion")]
    public Task<BalanceComprobacionDto> BalanceComprobacion(int empresaId, [FromQuery] DateTime desde, [FromQuery] DateTime hasta, CancellationToken ct)
        => _service.ObtenerBalanceComprobacionAsync(empresaId, desde, hasta, ct);

    [HttpGet("estado-resultados")]
    public Task<EstadoResultadosDto> EstadoResultados(int empresaId, [FromQuery] DateTime desde, [FromQuery] DateTime hasta, CancellationToken ct)
        => _service.ObtenerEstadoResultadosAsync(empresaId, desde, hasta, ct);

    [HttpGet("balance-general")]
    public Task<BalanceGeneralDto> BalanceGeneral(int empresaId, [FromQuery] DateTime hasta, CancellationToken ct)
        => _service.ObtenerBalanceGeneralAsync(empresaId, hasta, ct);

    [HttpGet("flujo-caja")]
    public Task<FlujoCajaDto> FlujoCaja(int empresaId, [FromQuery] DateTime desde, [FromQuery] DateTime hasta, CancellationToken ct)
        => _service.ObtenerFlujoCajaAsync(empresaId, desde, hasta, ct);

    [HttpGet("{reporte}/excel")]
    public async Task<IActionResult> Excel(int empresaId, string reporte, [FromQuery] DateTime desde, [FromQuery] DateTime hasta, CancellationToken ct)
    {
        var file = await _exportService.ExcelAsync(empresaId, reporte, desde, hasta, ct);
        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("{reporte}/pdf")]
    public async Task<IActionResult> Pdf(int empresaId, string reporte, [FromQuery] DateTime desde, [FromQuery] DateTime hasta, CancellationToken ct)
    {
        var file = await _exportService.PdfAsync(empresaId, reporte, desde, hasta, ct);
        return File(file.Content, file.ContentType, file.FileName);
    }
}
