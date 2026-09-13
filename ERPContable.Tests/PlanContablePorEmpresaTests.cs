using ERPContable.Application.Dtos;
using ERPContable.Infrastructure.Services;
using ERPContable.Domain.Entities;
using ERPContable.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPContable.Tests;

public sealed class PlanContablePorEmpresaTests
{
    [Fact]
    public async Task InicializarPlan_ClonaCuentasIndependientesPorEmpresa()
    {
        var options = new DbContextOptionsBuilder<ERPDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var context = new ERPDbContext(options);
        context.CuentasContables.AddRange(CuentaContable.Crear("10", "Efectivo", "ACTIVO"), CuentaContable.Crear("70", "Ventas", "INGRESO"));
        var empresaA = Empresa.Crear("Empresa A SAC", "20111111111");
        var empresaB = Empresa.Crear("Empresa B SAC", "20222222222");
        context.Empresas.AddRange(empresaA, empresaB);
        await context.SaveChangesAsync();

        var service = new ContabilidadService(context, null!);
        await service.InicializarPlanContableAsync(empresaA.Id);
        await service.InicializarPlanContableAsync(empresaB.Id);
        var cuentaA = (await service.GetCuentasAsync(empresaA.Id)).Single(x => x.Codigo == "10");
        var cuentaB = (await service.GetCuentasAsync(empresaB.Id)).Single(x => x.Codigo == "10");

        Assert.NotEqual(cuentaA.Id, cuentaB.Id);
        Assert.Equal(2, (await service.GetCuentasAsync(empresaA.Id)).Count);
        Assert.Equal(2, (await service.GetCuentasAsync(empresaB.Id)).Count);
    }

    [Fact]
    public async Task CrearCuenta_SoloLaExponeEnSuEmpresa()
    {
        var options = new DbContextOptionsBuilder<ERPDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var context = new ERPDbContext(options);
        var empresaA = Empresa.Crear("Empresa A SAC", "20333333333");
        var empresaB = Empresa.Crear("Empresa B SAC", "20444444444");
        context.Empresas.AddRange(empresaA, empresaB);
        await context.SaveChangesAsync();
        var service = new ContabilidadService(context, null!);

        await service.CrearCuentaAsync(empresaA.Id, new CuentaContableCreateDto("6311", "Energía eléctrica", "GASTO"));

        Assert.Single(await service.GetCuentasAsync(empresaA.Id));
        Assert.Empty(await service.GetCuentasAsync(empresaB.Id));
    }
}
