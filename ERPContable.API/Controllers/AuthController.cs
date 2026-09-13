using System.Security;
using System.Security.Claims;
using System.Security.Cryptography;
using ERPContable.Application.Dtos;
using ERPContable.Application.Interfaces;
using ERPContable.API.Services;
using ERPContable.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPContable.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private const string RefreshCookie = "erp_refresh";
    private const string CsrfCookie = "XSRF-TOKEN";
    private readonly IAuthService _authService;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public AuthController(IAuthService authService, IWebHostEnvironment environment, IConfiguration configuration)
    {
        _authService = authService;
        _environment = environment;
        _configuration = configuration;
    }

    [AllowAnonymous]
    [HttpPost("registro")]
    public async Task<ActionResult<RegistroResultadoDto>> Registro(RegistroUsuarioDto request, CancellationToken ct)
    {
        try
        {
            var result = await _authService.RegistrarAsync(request, Ip(), ct);
            if ((_environment.IsDevelopment() || _environment.IsEnvironment("Desktop")) && _configuration.GetValue("DemoData:Enabled", true))
                await DemoDataSeeder.SeedAsync(HttpContext.RequestServices, ct);
            return Ok(result);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [AllowAnonymous]
    [HttpPost("inicio-sesion")]
    public async Task<ActionResult<SesionAutenticadaDto>> InicioSesion(InicioSesionDto request, CancellationToken ct)
    {
        try
        {
            var session = await _authService.IniciarSesionAsync(request, Ip(), ct);
            EstablecerCookies(session.RefreshToken);
            return Ok(session);
        }
        catch (SecurityException ex) { return Unauthorized(new { message = ex.Message }); }
    }

    [AllowAnonymous]
    [HttpPost("renovar")]
    public async Task<ActionResult<SesionAutenticadaDto>> Renovar(CancellationToken ct)
    {
        if (!CsrfValido()) return BadRequest(new { message = "Validación CSRF no válida." });
        try
        {
            var token = Request.Cookies[RefreshCookie];
            if (string.IsNullOrWhiteSpace(token)) return Unauthorized(new { message = "No hay una sesión para renovar." });
            var session = await _authService.RenovarSesionAsync(token, Ip(), ct);
            EstablecerCookies(session.RefreshToken);
            return Ok(session);
        }
        catch (SecurityException ex) { LimpiarCookies(); return Unauthorized(new { message = ex.Message }); }
    }

    [Authorize]
    [HttpGet("sesion")]
    public async Task<ActionResult<SesionAutenticadaDto>> Sesion(CancellationToken ct)
    {
        var session = await _authService.ObtenerSesionAsync(UserId(), EmpresaActiva(), ct);
        return session is null ? Unauthorized() : Ok(session);
    }

    [Authorize]
    [HttpPost("seleccionar-empresa")]
    public async Task<ActionResult<SesionAutenticadaDto>> SeleccionarEmpresa(SeleccionarEmpresaDto request, CancellationToken ct)
    {
        try
        {
            var session = await _authService.SeleccionarEmpresaAsync(UserId(), request, Request.Cookies[RefreshCookie], Ip(), ct);
            EstablecerCookies(session.RefreshToken);
            return Ok(session);
        }
        catch (SecurityException) { return Forbid(); }
    }

    [AllowAnonymous]
    [HttpPost("cerrar-sesion")]
    public async Task<IActionResult> CerrarSesion(CancellationToken ct)
    {
        if (!CsrfValido()) return BadRequest(new { message = "Validación CSRF no válida." });
        await _authService.CerrarSesionAsync(Request.Cookies[RefreshCookie], User.FindFirstValue(ClaimTypes.NameIdentifier), Ip(), ct);
        LimpiarCookies();
        return NoContent();
    }

    [Authorize]
    [HttpPost("cambiar-contrasena")]
    public async Task<IActionResult> CambiarContrasena(CambiarContrasenaDto request, CancellationToken ct)
    {
        try
        {
            await _authService.CambiarContrasenaAsync(UserId(), request, Ip(), ct);
            LimpiarCookies();
            return NoContent();
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [AllowAnonymous]
    [HttpPost("recuperar-contrasena")]
    public async Task<IActionResult> RecuperarContrasena(SolicitarRecuperacionDto request, CancellationToken ct)
    {
        await _authService.SolicitarRecuperacionAsync(request, ct);
        return Accepted();
    }

    [AllowAnonymous]
    [HttpPost("restablecer-contrasena")]
    public async Task<IActionResult> RestablecerContrasena(RestablecerContrasenaDto request, CancellationToken ct)
    {
        try { await _authService.RestablecerContrasenaAsync(request, Ip(), ct); return NoContent(); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [AllowAnonymous]
    [HttpPost("confirmar-correo")]
    public async Task<IActionResult> ConfirmarCorreo([FromQuery] string userId, [FromQuery] string token, CancellationToken ct)
    {
        try { await _authService.ConfirmarCorreoAsync(userId, token, ct); return NoContent(); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    private void EstablecerCookies(string refreshToken)
    {
        var secure = !_environment.IsDevelopment() && !_environment.IsEnvironment("Desktop");
        Response.Cookies.Append(RefreshCookie, refreshToken, new CookieOptions { HttpOnly = true, Secure = secure, SameSite = SameSiteMode.Strict, Path = "/api/auth", MaxAge = TimeSpan.FromDays(14) });
        Response.Cookies.Append(CsrfCookie, Convert.ToHexString(RandomNumberGenerator.GetBytes(32)), new CookieOptions { HttpOnly = false, Secure = secure, SameSite = SameSiteMode.Strict, Path = "/", MaxAge = TimeSpan.FromDays(14) });
    }

    private void LimpiarCookies()
    {
        Response.Cookies.Delete(RefreshCookie, new CookieOptions { Path = "/api/auth" });
        Response.Cookies.Delete(CsrfCookie, new CookieOptions { Path = "/" });
    }

    private bool CsrfValido()
    {
        var cookie = Request.Cookies[CsrfCookie];
        var header = Request.Headers["X-CSRF-TOKEN"].ToString();
        return !string.IsNullOrWhiteSpace(cookie) && CryptographicOperations.FixedTimeEquals(System.Text.Encoding.UTF8.GetBytes(cookie), System.Text.Encoding.UTF8.GetBytes(header));
    }

    private string UserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new SecurityException("Sesión no válida.");
    private int? EmpresaActiva() => int.TryParse(User.FindFirstValue(AuthService.EmpresaClaim), out var id) ? id : null;
    private string? Ip() => HttpContext.Connection.RemoteIpAddress?.ToString();
}
