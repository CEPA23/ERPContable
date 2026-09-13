using ERPContable.Application.Dtos;

namespace ERPContable.Application.Interfaces;

public interface IEmpresaAccesoService
{
    Task<IReadOnlyList<EmpresaDto>> ObtenerParaUsuarioAsync(string usuarioId, CancellationToken cancellationToken = default);
    Task<EmpresaDto?> ObtenerParaUsuarioAsync(string usuarioId, int empresaId, CancellationToken cancellationToken = default);
    Task<EmpresaDto> CrearParaUsuarioAsync(string usuarioId, EmpresaCreateDto request, CancellationToken cancellationToken = default);
    Task<bool> ActualizarParaUsuarioAsync(string usuarioId, int empresaId, EmpresaUpdateDto request, CancellationToken cancellationToken = default);
    Task<bool> EliminarParaUsuarioAsync(string usuarioId, int empresaId, CancellationToken cancellationToken = default);
}
