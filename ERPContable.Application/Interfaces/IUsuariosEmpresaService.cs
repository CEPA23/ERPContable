using ERPContable.Application.Dtos;

namespace ERPContable.Application.Interfaces;

public interface IUsuariosEmpresaService
{
    Task<IReadOnlyList<UsuarioEmpresaDto>> ObtenerAsync(int empresaId, CancellationToken cancellationToken = default);
    Task AgregarAsync(int empresaId, AgregarUsuarioEmpresaDto request, CancellationToken cancellationToken = default);
    Task ActualizarAsync(int empresaId, string usuarioId, ActualizarUsuarioEmpresaDto request, CancellationToken cancellationToken = default);
}
