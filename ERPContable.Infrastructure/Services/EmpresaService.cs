using ERPContable.Application.Dtos;
using ERPContable.Application.Interfaces;
using ERPContable.Domain.Entities;
using ERPContable.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPContable.Infrastructure.Services;

public sealed class EmpresaService : IEmpresaService
{
    private readonly ERPDbContext _dbContext;

    public EmpresaService(ERPDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<EmpresaDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var empresas = await _dbContext.Empresas
            .AsNoTracking()
            .OrderBy(x => x.RazonSocial)
            .ToListAsync(cancellationToken);

        return empresas.Select(MapToDto).ToList();
    }

    public async Task<EmpresaDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var empresa = await _dbContext.Empresas
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return empresa is null ? null : MapToDto(empresa);
    }

    public async Task<EmpresaDto> CreateAsync(EmpresaCreateDto request, CancellationToken cancellationToken = default)
    {
        var empresa = Empresa.Crear(
            request.RazonSocial,
            request.DocumentoIdentidad,
            request.NombreComercial,
            request.Direccion,
            request.Telefono,
            request.Email,
            request.EstadoSunat,
            request.CondicionSunat);

        _dbContext.Empresas.Add(empresa);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(empresa);
    }

    public async Task<bool> UpdateAsync(int id, EmpresaUpdateDto request, CancellationToken cancellationToken = default)
    {
        var empresa = await _dbContext.Empresas.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (empresa is null)
        {
            return false;
        }

        empresa.Actualizar(
            request.RazonSocial,
            request.DocumentoIdentidad,
            request.NombreComercial,
            request.Direccion,
            request.Telefono,
            request.Email,
            request.Activa,
            request.EstadoSunat,
            request.CondicionSunat);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var empresa = await _dbContext.Empresas.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (empresa is null)
        {
            return false;
        }

        _dbContext.Empresas.Remove(empresa);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static EmpresaDto MapToDto(Empresa empresa)
        => new(
            empresa.Id,
            empresa.RazonSocial,
            empresa.DocumentoIdentidad,
            empresa.NombreComercial,
            empresa.Direccion,
            empresa.Telefono,
            empresa.Email,
            empresa.Activa,
            empresa.CreadaEnUtc,
            empresa.ActualizadaEnUtc,
            empresa.EstadoSunat,
            empresa.CondicionSunat);
}
