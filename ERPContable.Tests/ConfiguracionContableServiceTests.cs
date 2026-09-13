using ERPContable.Application.Dtos;
using ERPContable.Domain.Entities;
using ERPContable.Infrastructure.Services;
using ERPContable.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPContable.Tests;

public sealed class ConfiguracionContableServiceTests
{
    [Fact]
    public async Task ActualizarGuardaLasTresCuentasAutomaticas()
    {
        await using var context = CreateContext();
        var company = Empresa.Crear("Empresa de configuración", "20777777777");
        context.Empresas.Add(company);
        await context.SaveChangesAsync();

        var retained = CuentaContable.CrearParaEmpresa(company.Id, "59", "Resultados acumulados", "PATRIMONIO");
        var inventory = CuentaContable.CrearParaEmpresa(company.Id, "20", "Mercaderías", "ACTIVO");
        var cost = CuentaContable.CrearParaEmpresa(company.Id, "69", "Costo de ventas", "GASTO");
        context.CuentasContables.AddRange(retained, inventory, cost);
        await context.SaveChangesAsync();

        var service = new ConfiguracionContableService(context);
        var result = await service.ActualizarAsync(company.Id, new ActualizarConfiguracionContableDto(retained.Id, inventory.Id, cost.Id));

        Assert.Equal(retained.Id, result.CuentaResultadoAcumuladoId);
        Assert.Equal(inventory.Id, result.CuentaInventarioId);
        Assert.Equal(cost.Id, result.CuentaCostoVentasId);
        Assert.Equal("20 - Mercaderías", result.CuentaInventario);
    }

    [Fact]
    public async Task ActualizarRechazaCuentaConNaturalezaIncorrecta()
    {
        await using var context = CreateContext();
        var company = Empresa.Crear("Empresa de validación", "20766666666");
        context.Empresas.Add(company);
        await context.SaveChangesAsync();

        var retained = CuentaContable.CrearParaEmpresa(company.Id, "59", "Resultados acumulados", "PATRIMONIO");
        var inventory = CuentaContable.CrearParaEmpresa(company.Id, "20", "Mercaderías", "ACTIVO");
        var wrongCost = CuentaContable.CrearParaEmpresa(company.Id, "12", "Clientes", "ACTIVO");
        context.CuentasContables.AddRange(retained, inventory, wrongCost);
        await context.SaveChangesAsync();

        var service = new ConfiguracionContableService(context);

        var error = await Assert.ThrowsAsync<ArgumentException>(() => service.ActualizarAsync(company.Id, new ActualizarConfiguracionContableDto(retained.Id, inventory.Id, wrongCost.Id)));

        Assert.Contains("GASTO", error.Message);
    }

    private static ERPDbContext CreateContext()
        => new(new DbContextOptionsBuilder<ERPDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
}
