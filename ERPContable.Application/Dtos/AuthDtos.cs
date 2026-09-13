using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ERPContable.Application.Dtos;

public sealed record RegistroUsuarioDto(
    [param: Required, StringLength(200, MinimumLength = 3)] string NombreCompleto,
    [param: Required, EmailAddress, StringLength(254)] string CorreoElectronico,
    [param: Required, StringLength(128, MinimumLength = 10)] string Contrasena);

public sealed record InicioSesionDto(
    [param: Required, EmailAddress] string CorreoElectronico,
    [param: Required] string Contrasena);

public sealed record SeleccionarEmpresaDto([param: Range(1, int.MaxValue)] int EmpresaId);

public sealed record UsuarioSesionDto(string Id, string NombreCompleto, string CorreoElectronico, bool CorreoConfirmado);

public sealed record EmpresaSesionDto(EmpresaDto Empresa, string Rol);

public sealed record SesionAutenticadaDto(
    string AccessToken,
    DateTime ExpiraEnUtc,
    UsuarioSesionDto Usuario,
    IReadOnlyList<EmpresaSesionDto> Empresas,
    EmpresaSesionDto? EmpresaActiva,
    [property: JsonIgnore] string RefreshToken);

public sealed record RegistroResultadoDto(string Mensaje, bool RequiereConfirmacionCorreo);

public sealed record CambiarContrasenaDto(
    [param: Required] string ContrasenaActual,
    [param: Required, StringLength(128, MinimumLength = 10)] string NuevaContrasena);

public sealed record SolicitarRecuperacionDto([param: Required, EmailAddress] string CorreoElectronico);

public sealed record RestablecerContrasenaDto(
    [param: Required, EmailAddress] string CorreoElectronico,
    [param: Required] string Token,
    [param: Required, StringLength(128, MinimumLength = 10)] string NuevaContrasena);

public sealed record UsuarioEmpresaDto(string UsuarioId, string NombreCompleto, string CorreoElectronico, string Rol, bool Activo);

public sealed record AgregarUsuarioEmpresaDto(
    [param: Required, EmailAddress] string CorreoElectronico,
    [param: Required] string Rol);

public sealed record ActualizarUsuarioEmpresaDto([param: Required] string Rol, bool Activo);
