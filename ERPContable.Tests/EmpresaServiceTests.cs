using ERPContable.Application.Dtos;
using ERPContable.Infrastructure.Services;
using ERPContable.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPContable.Tests;

public sealed class EmpresaServiceTests
{
    private static ERPDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ERPDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ERPDbContext(options);
    }

    [Fact]
    public async Task CreateAsync_CreaEmpresaYLaDevuelve()
    {
        await using var context = CreateContext();
        var service = new EmpresaService(context);

        var created = await service.CreateAsync(new EmpresaCreateDto(
            "ERP Contable SAC",
            "20123456789",
            "ERP Contable",
            "Av. Principal 123",
            "999888777",
            "contacto@erpcontable.com"));

        Assert.True(created.Id > 0);
        Assert.Equal("ERP Contable SAC", created.RazonSocial);
        Assert.True(created.Activa);

        var stored = await context.Empresas.SingleAsync();
        Assert.Equal(created.Id, stored.Id);
    }

    [Fact]
    public async Task GetAllAsync_RetornaEmpresasOrdenadasPorRazonSocial()
    {
        await using var context = CreateContext();
        var service = new EmpresaService(context);

        await service.CreateAsync(new EmpresaCreateDto("Zeta SAC", "20111111111", null, null, null, null));
        await service.CreateAsync(new EmpresaCreateDto("Alpha SAC", "20222222222", null, null, null, null));

        var empresas = await service.GetAllAsync();

        Assert.Equal(2, empresas.Count);
        Assert.Equal("Alpha SAC", empresas[0].RazonSocial);
        Assert.Equal("Zeta SAC", empresas[1].RazonSocial);
    }

    [Fact]
    public async Task UpdateAsync_ActualizaEmpresaExistente()
    {
        await using var context = CreateContext();
        var service = new EmpresaService(context);

        var created = await service.CreateAsync(new EmpresaCreateDto("Inicio SAC", "20999999999", null, null, null, null));

        var updated = await service.UpdateAsync(created.Id, new EmpresaUpdateDto(
            "Inicio Actualizado SAC",
            "20999999999",
            "Inicio",
            "Jr. Central 456",
            "987654321",
            "info@inicio.com",
            false));

        Assert.True(updated);

        var empresa = await service.GetByIdAsync(created.Id);
        Assert.NotNull(empresa);
        Assert.Equal("Inicio Actualizado SAC", empresa!.RazonSocial);
        Assert.False(empresa.Activa);
    }

    [Fact]
    public async Task DeleteAsync_EliminaEmpresaExistente()
    {
        await using var context = CreateContext();
        var service = new EmpresaService(context);

        var created = await service.CreateAsync(new EmpresaCreateDto("Borrar SAC", "20333333333", null, null, null, null));

        var deleted = await service.DeleteAsync(created.Id);

        Assert.True(deleted);
        Assert.Null(await service.GetByIdAsync(created.Id));
    }

    [Fact]
    public async Task UpdateAsync_CuandoNoExiste_RetornaFalse()
    {
        await using var context = CreateContext();
        var service = new EmpresaService(context);

        var result = await service.UpdateAsync(999, new EmpresaUpdateDto(
            "No existe SAC",
            "20000000000",
            null,
            null,
            null,
            null,
            true));

        Assert.False(result);
    }
}
