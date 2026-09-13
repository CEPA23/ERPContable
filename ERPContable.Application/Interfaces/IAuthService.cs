using ERPContable.Application.Dtos;

namespace ERPContable.Application.Interfaces;

public interface IAuthService
{
    Task<RegistroResultadoDto> RegistrarAsync(RegistroUsuarioDto request, string? direccionIp, CancellationToken cancellationToken = default);
    Task<SesionAutenticadaDto> IniciarSesionAsync(InicioSesionDto request, string? direccionIp, CancellationToken cancellationToken = default);
    Task<SesionAutenticadaDto> RenovarSesionAsync(string refreshToken, string? direccionIp, CancellationToken cancellationToken = default);
    Task<SesionAutenticadaDto> SeleccionarEmpresaAsync(string usuarioId, SeleccionarEmpresaDto request, string? refreshTokenActual, string? direccionIp, CancellationToken cancellationToken = default);
    Task<SesionAutenticadaDto?> ObtenerSesionAsync(string usuarioId, int? empresaId, CancellationToken cancellationToken = default);
    Task CerrarSesionAsync(string? refreshToken, string? usuarioId, string? direccionIp, CancellationToken cancellationToken = default);
    Task CambiarContrasenaAsync(string usuarioId, CambiarContrasenaDto request, string? direccionIp, CancellationToken cancellationToken = default);
    Task SolicitarRecuperacionAsync(SolicitarRecuperacionDto request, CancellationToken cancellationToken = default);
    Task RestablecerContrasenaAsync(RestablecerContrasenaDto request, string? direccionIp, CancellationToken cancellationToken = default);
    Task ConfirmarCorreoAsync(string usuarioId, string token, CancellationToken cancellationToken = default);
}
