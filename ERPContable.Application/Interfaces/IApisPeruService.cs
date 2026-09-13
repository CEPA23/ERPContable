using ERPContable.Application.Dtos;

namespace ERPContable.Application.Interfaces;

public interface IApisPeruService
{
    Task<ConsultaDocumentoDto?> ConsultarRucAsync(string ruc, CancellationToken cancellationToken = default);
    Task<ConsultaDocumentoDto?> ConsultarDniAsync(string dni, CancellationToken cancellationToken = default);
}
