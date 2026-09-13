using System.IdentityModel.Tokens.Jwt;
using System.Security;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ERPContable.Application.Dtos;
using ERPContable.Application.Interfaces;
using ERPContable.Domain.Entities;
using ERPContable.Persistence.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ERPContable.Infrastructure.Services;

public sealed class AuthService : IAuthService
{
    public const string EmpresaClaim = "empresa_id";
    public const string SecurityStampClaim = "security_stamp";
    private readonly ERPDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly IAuditoriaService _auditoria;

    public AuthService(ERPDbContext dbContext, UserManager<ApplicationUser> userManager, IConfiguration configuration, IEmailService emailService, IAuditoriaService auditoria)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _configuration = configuration;
        _emailService = emailService;
        _auditoria = auditoria;
    }

    public async Task<RegistroResultadoDto> RegistrarAsync(RegistroUsuarioDto request, string? direccionIp, CancellationToken cancellationToken = default)
    {
        var email = request.CorreoElectronico.Trim().ToLowerInvariant();
        var esPrimerUsuario = !await _dbContext.Users.AnyAsync(cancellationToken);
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            NombreCompleto = request.NombreCompleto.Trim(),
            LockoutEnabled = true,
            Activo = true,
            CreadoEnUtc = DateTime.UtcNow
        };
        var result = await _userManager.CreateAsync(user, request.Contrasena);
        if (!result.Succeeded) throw new ArgumentException(string.Join(" ", result.Errors.Select(x => x.Description)));

        if (esPrimerUsuario)
        {
            var empresasExistentes = await _dbContext.Empresas.Select(x => x.Id).ToListAsync(cancellationToken);
            foreach (var empresaId in empresasExistentes)
                _dbContext.UsuariosEmpresas.Add(new UsuarioEmpresa { UsuarioId = user.Id, EmpresaId = empresaId, Rol = RolesERP.Administrador, Activo = true, CreadoEnUtc = DateTime.UtcNow });
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        await EnviarConfirmacionCorreoAsync(user, token, cancellationToken);
        await _auditoria.RegistrarAsync("REGISTRO_USUARIO", "Usuario", user.Id, null, direccionIp, "Usuario registrado", cancellationToken);
        return new RegistroResultadoDto("Registro completado. Revisa tu correo para confirmar la cuenta.", RequiereConfirmacionCorreo());
    }

    public async Task<SesionAutenticadaDto> IniciarSesionAsync(InicioSesionDto request, string? direccionIp, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.CorreoElectronico.Trim());
        if (user is null || !user.Activo) throw new SecurityException("Correo o contraseña incorrectos.");
        if (await _userManager.IsLockedOutAsync(user)) throw new SecurityException("La cuenta se encuentra bloqueada temporalmente. Intenta más tarde.");
        if (RequiereConfirmacionCorreo() && !user.EmailConfirmed) throw new SecurityException("Debes confirmar tu correo electrónico antes de ingresar.");
        if (!await _userManager.CheckPasswordAsync(user, request.Contrasena))
        {
            await _userManager.AccessFailedAsync(user);
            throw new SecurityException("Correo o contraseña incorrectos.");
        }

        await _userManager.ResetAccessFailedCountAsync(user);
        user.UltimoAccesoEnUtc = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
        var session = await EmitirSesionAsync(user, null, null, direccionIp, cancellationToken);
        await _auditoria.RegistrarAsync("INICIO_SESION", "Sesion", user.Id, null, direccionIp, "Inicio de sesión correcto", cancellationToken);
        return session;
    }

    public async Task<SesionAutenticadaDto> RenovarSesionAsync(string refreshToken, string? direccionIp, CancellationToken cancellationToken = default)
    {
        var current = await _dbContext.RefreshTokens.Include(x => x.Usuario)
            .FirstOrDefaultAsync(x => x.TokenHash == Hash(refreshToken), cancellationToken);
        if (current is null || current.RevocadoEnUtc is not null || current.ExpiraEnUtc <= DateTime.UtcNow || !current.Usuario.Activo)
            throw new SecurityException("La sesión expiró. Ingresa nuevamente.");

        current.RevocadoEnUtc = DateTime.UtcNow;
        var membership = current.EmpresaId is null ? null : await _dbContext.UsuariosEmpresas.Include(x => x.Empresa)
            .FirstOrDefaultAsync(x => x.UsuarioId == current.UsuarioId && x.EmpresaId == current.EmpresaId && x.Activo && x.Empresa.Activa, cancellationToken);
        if (current.EmpresaId is not null && membership is null)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            throw new SecurityException("Ya no tienes acceso a la empresa seleccionada.");
        }
        var session = await EmitirSesionAsync(current.Usuario, membership?.EmpresaId, membership?.Rol, direccionIp, cancellationToken);
        current.ReemplazadoPorHash = Hash(session.RefreshToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return session;
    }

    public async Task<SesionAutenticadaDto> SeleccionarEmpresaAsync(string usuarioId, SeleccionarEmpresaDto request, string? refreshTokenActual, string? direccionIp, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(usuarioId) ?? throw new SecurityException("Usuario no válido.");
        var membership = await _dbContext.UsuariosEmpresas.Include(x => x.Empresa)
            .FirstOrDefaultAsync(x => x.UsuarioId == usuarioId && x.EmpresaId == request.EmpresaId && x.Activo && x.Empresa.Activa, cancellationToken)
            ?? throw new SecurityException("No tienes acceso a esta empresa.");
        if (!string.IsNullOrWhiteSpace(refreshTokenActual))
        {
            var previous = await _dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == Hash(refreshTokenActual) && x.UsuarioId == usuarioId, cancellationToken);
            if (previous is not null && previous.RevocadoEnUtc is null) previous.RevocadoEnUtc = DateTime.UtcNow;
        }
        var session = await EmitirSesionAsync(user, membership.EmpresaId, membership.Rol, direccionIp, cancellationToken);
        await _auditoria.RegistrarAsync("SELECCION_EMPRESA", "Empresa", user.Id, membership.EmpresaId, direccionIp, "Empresa seleccionada", cancellationToken);
        return session;
    }

    public async Task<SesionAutenticadaDto?> ObtenerSesionAsync(string usuarioId, int? empresaId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(usuarioId);
        if (user is null || !user.Activo) return null;
        var memberships = await ObtenerEmpresasAsync(usuarioId, cancellationToken);
        var active = empresaId is null ? null : memberships.FirstOrDefault(x => x.Empresa.Id == empresaId);
        if (empresaId is not null && active is null) return null;
        return new SesionAutenticadaDto(string.Empty, DateTime.UtcNow, MapUser(user), memberships, active, string.Empty);
    }

    public async Task CerrarSesionAsync(string? refreshToken, string? usuarioId, string? direccionIp, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            var token = await _dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == Hash(refreshToken), cancellationToken);
            if (token is not null && token.RevocadoEnUtc is null)
            {
                token.RevocadoEnUtc = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync(cancellationToken);
                usuarioId ??= token.UsuarioId;
            }
        }
        await _auditoria.RegistrarAsync("CIERRE_SESION", "Sesion", usuarioId, null, direccionIp, "Cierre de sesión", cancellationToken);
    }

    public async Task CambiarContrasenaAsync(string usuarioId, CambiarContrasenaDto request, string? direccionIp, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(usuarioId) ?? throw new SecurityException("Usuario no válido.");
        var result = await _userManager.ChangePasswordAsync(user, request.ContrasenaActual, request.NuevaContrasena);
        if (!result.Succeeded) throw new ArgumentException(string.Join(" ", result.Errors.Select(x => x.Description)));
        await _userManager.UpdateSecurityStampAsync(user);
        await RevocarTokensAsync(user.Id, cancellationToken);
        await _auditoria.RegistrarAsync("CAMBIO_CONTRASENA", "Usuario", user.Id, null, direccionIp, "Contraseña actualizada", cancellationToken);
    }

    public async Task SolicitarRecuperacionAsync(SolicitarRecuperacionDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.CorreoElectronico.Trim());
        if (user is null || !user.Activo) return;
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var url = $"{FrontendUrl()}/restablecer-contrasena?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";
        await _emailService.EnviarAsync(user.Email!, "Restablece tu contraseña", $"<p>Solicitaste restablecer tu contraseña.</p><p><a href=\"{url}\">Restablecer contraseña</a></p>", cancellationToken);
    }

    public async Task RestablecerContrasenaAsync(RestablecerContrasenaDto request, string? direccionIp, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.CorreoElectronico.Trim()) ?? throw new ArgumentException("Solicitud no válida.");
        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NuevaContrasena);
        if (!result.Succeeded) throw new ArgumentException("El enlace de recuperación no es válido o expiró.");
        await _userManager.UpdateSecurityStampAsync(user);
        await RevocarTokensAsync(user.Id, cancellationToken);
        await _auditoria.RegistrarAsync("RESTABLECER_CONTRASENA", "Usuario", user.Id, null, direccionIp, "Contraseña restablecida", cancellationToken);
    }

    public async Task ConfirmarCorreoAsync(string usuarioId, string token, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(usuarioId) ?? throw new ArgumentException("Usuario no encontrado.");
        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded) throw new ArgumentException("El enlace de confirmación no es válido o expiró.");
        await _auditoria.RegistrarAsync("CONFIRMACION_CORREO", "Usuario", user.Id, null, null, "Correo confirmado", cancellationToken);
    }

    private async Task<SesionAutenticadaDto> EmitirSesionAsync(ApplicationUser user, int? empresaId, string? rol, string? direccionIp, CancellationToken cancellationToken)
    {
        var memberships = await ObtenerEmpresasAsync(user.Id, cancellationToken);
        var active = empresaId is null ? null : memberships.FirstOrDefault(x => x.Empresa.Id == empresaId);
        var expires = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int?>("Jwt:AccessTokenMinutes") ?? 15);
        var accessToken = CrearAccessToken(user, active, expires);
        var refresh = CrearRefreshToken();
        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            UsuarioId = user.Id, TokenHash = Hash(refresh), EmpresaId = active?.Empresa.Id, RolEmpresa = active?.Rol,
            CreadoEnUtc = DateTime.UtcNow, ExpiraEnUtc = DateTime.UtcNow.AddDays(_configuration.GetValue<int?>("Jwt:RefreshTokenDays") ?? 14), DireccionIp = direccionIp
        });
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new SesionAutenticadaDto(accessToken, expires, MapUser(user), memberships, active, refresh);
    }

    private string CrearAccessToken(ApplicationUser user, EmpresaSesionDto? active, DateTime expires)
    {
        var key = _configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(key) || key.Length < 32) throw new InvalidOperationException("La clave Jwt:Key debe configurarse como un secreto de al menos 32 caracteres.");
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, user.Id), new(ClaimTypes.Name, user.NombreCompleto), new(ClaimTypes.Email, user.Email ?? string.Empty), new(SecurityStampClaim, user.SecurityStamp ?? string.Empty) };
        if (active is not null) { claims.Add(new Claim(EmpresaClaim, active.Empresa.Id.ToString())); claims.Add(new Claim(ClaimTypes.Role, active.Rol)); }
        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(_configuration["Jwt:Issuer"], _configuration["Jwt:Audience"], claims, DateTime.UtcNow, expires, credentials));
    }

    private async Task<IReadOnlyList<EmpresaSesionDto>> ObtenerEmpresasAsync(string userId, CancellationToken cancellationToken)
        => await _dbContext.UsuariosEmpresas.AsNoTracking().Include(x => x.Empresa)
            .Where(x => x.UsuarioId == userId && x.Activo && x.Empresa.Activa).OrderBy(x => x.Empresa.RazonSocial)
            .Select(x => new EmpresaSesionDto(new EmpresaDto(x.Empresa.Id, x.Empresa.RazonSocial, x.Empresa.DocumentoIdentidad, x.Empresa.NombreComercial, x.Empresa.Direccion, x.Empresa.Telefono, x.Empresa.Email, x.Empresa.Activa, x.Empresa.CreadaEnUtc, x.Empresa.ActualizadaEnUtc, x.Empresa.EstadoSunat, x.Empresa.CondicionSunat), x.Rol)).ToListAsync(cancellationToken);

    private async Task RevocarTokensAsync(string userId, CancellationToken cancellationToken)
    {
        var tokens = await _dbContext.RefreshTokens.Where(x => x.UsuarioId == userId && x.RevocadoEnUtc == null).ToListAsync(cancellationToken);
        foreach (var token in tokens) token.RevocadoEnUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnviarConfirmacionCorreoAsync(ApplicationUser user, string token, CancellationToken cancellationToken)
    {
        var url = $"{FrontendUrl()}/confirmar-correo?userId={Uri.EscapeDataString(user.Id)}&token={Uri.EscapeDataString(token)}";
        await _emailService.EnviarAsync(user.Email!, "Confirma tu correo", $"<p>Confirma tu correo para activar la cuenta.</p><p><a href=\"{url}\">Confirmar correo</a></p>", cancellationToken);
    }

    private bool RequiereConfirmacionCorreo() => _configuration.GetValue("Auth:RequireConfirmedEmail", true);
    private string FrontendUrl() => _configuration["App:FrontendUrl"]?.TrimEnd('/') ?? "http://localhost:5173";
    private static UsuarioSesionDto MapUser(ApplicationUser user) => new(user.Id, user.NombreCompleto, user.Email ?? string.Empty, user.EmailConfirmed);
    private static string CrearRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
