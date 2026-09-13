using ERPContable.Persistence.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPContable.API.Controllers;

[ApiController]
[Route("api/system")]
public sealed class SystemController : ControllerBase
{
    private readonly ERPDbContext _db;
    public SystemController(ERPDbContext db) => _db = db;

    [HttpGet("health")]
    [AllowAnonymous]
    public async Task<IActionResult> Health(CancellationToken ct)
    {
        try
        {
            var database = await _db.Database.CanConnectAsync(ct);
            return database ? Ok(new { api = "operativa", database = "conectada", status = "ok" }) : StatusCode(503, new { api = "operativa", database = "desconectada", status = "degraded" });
        }
        catch
        {
            return StatusCode(503, new { api = "operativa", database = "desconectada", status = "degraded" });
        }
    }
}
