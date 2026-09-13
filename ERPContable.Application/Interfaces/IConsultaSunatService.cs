using ERPContable.Application.Dtos;

namespace ERPContable.Application.Interfaces;

public interface IConsultaSunatService
{
    Task<SunatEmpresaDto?> ConsultarEmpresaAsync(string ruc, CancellationToken cancellationToken = default);
}
