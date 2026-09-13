using ERPContable.Application.Dtos;
using ERPContable.Application.Interfaces;

namespace ERPContable.Infrastructure.Services;

public sealed class ConsultaSunatService : IConsultaSunatService
{
    private readonly IApisPeruService _provider;
    public ConsultaSunatService(IApisPeruService provider) => _provider = provider;

    public async Task<SunatEmpresaDto?> ConsultarEmpresaAsync(string ruc, CancellationToken cancellationToken = default)
    {
        var result = await _provider.ConsultarRucAsync(ruc, cancellationToken);
        return result?.Ruc is null ? null : new SunatEmpresaDto(result.Ruc, result.RazonSocial, result.Direccion, result.Estado, result.Condicion);
    }
}
