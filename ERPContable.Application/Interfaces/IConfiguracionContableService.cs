using ERPContable.Application.Dtos;

namespace ERPContable.Application.Interfaces;

public interface IConfiguracionContableService
{
    Task<ConfiguracionContableDto> ObtenerAsync(int empresaId, CancellationToken cancellationToken = default);
    Task<ConfiguracionContableDto> ActualizarAsync(int empresaId, ActualizarConfiguracionContableDto request, CancellationToken cancellationToken = default);
}
