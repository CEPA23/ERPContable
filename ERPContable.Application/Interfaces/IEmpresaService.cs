using ERPContable.Application.Dtos;

namespace ERPContable.Application.Interfaces;

public interface IEmpresaService
{
    Task<IReadOnlyList<EmpresaDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<EmpresaDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<EmpresaDto> CreateAsync(EmpresaCreateDto request, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(int id, EmpresaUpdateDto request, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
