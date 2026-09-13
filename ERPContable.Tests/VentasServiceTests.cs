using ERPContable.Application.Dtos;
using ERPContable.Domain.Entities;
using ERPContable.Infrastructure.Services;
using ERPContable.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPContable.Tests;

public sealed class VentasServiceTests
{
    [Fact]
    public async Task CrearVentaConProducto_GeneraCostoDeVentasYDescuentaStock()
    {
        await using var context = CreateContext();
        var company = Empresa.Crear("Empresa de prueba", "20999999999");
        context.Empresas.Add(company);
        await context.SaveChangesAsync();

        var cash = CuentaContable.CrearParaEmpresa(company.Id, "10", "Efectivo", "ACTIVO");
        var tax = CuentaContable.CrearParaEmpresa(company.Id, "4011", "IGV", "PASIVO");
        var sales = CuentaContable.CrearParaEmpresa(company.Id, "70", "Ventas", "INGRESO");
        var inventory = CuentaContable.CrearParaEmpresa(company.Id, "20", "Mercaderías", "ACTIVO");
        var cost = CuentaContable.CrearParaEmpresa(company.Id, "69", "Costo de ventas", "GASTO");
        var retained = CuentaContable.CrearParaEmpresa(company.Id, "59", "Resultados acumulados", "PATRIMONIO");
        var unit = UnidadMedida.Crear(company.Id, "UND", "Unidad", "UND");
        var taxDefinition = Impuesto.Crear(company.Id, "IGV", "IGV", 18m);
        var customer = Cliente.Crear(company.Id, "10444444444", "Cliente de prueba");
        context.CuentasContables.AddRange(cash, tax, sales, inventory, cost, retained);
        context.UnidadesMedida.Add(unit);
        context.Impuestos.Add(taxDefinition);
        context.Clientes.Add(customer);
        await context.SaveChangesAsync();
        context.ConfiguracionesContablesEmpresas.Add(ConfiguracionContableEmpresa.Crear(company.Id, retained.Id, inventory.Id, cost.Id));
        await context.SaveChangesAsync();

        var product = ProductoServicio.Crear(company.Id, "PRODUCTO", "PROD-001", "Producto de prueba", null, null, unit.Id, taxDefinition.Id, 150m, 100m);
        context.ProductosServicios.Add(product);
        await context.SaveChangesAsync();
        var stock = InventarioProducto.Crear(company.Id, product.Id);
        stock.AplicarMovimiento(10m, 100m);
        context.InventarioProductos.Add(stock);
        await context.SaveChangesAsync();

        var service = new VentasService(context, new CierreContableService(context), new InventarioService(context, new CierreContableService(context)));
        var created = await service.CrearVentaAsync(company.Id, new VentaCreateDto(
            DateTime.Today, customer.Id, "FACTURA", "F001", "00000001", 0m, "CONTADO", cash.Id, tax.Id,
            [new VentaDetalleCreateDto(sales.Id, "Producto de prueba", 2m, 150m, product.Id)]));

        Assert.NotNull(created.AsientoContableId);
        Assert.NotNull(created.AsientoCostoVentaId);
        var storedStock = await context.InventarioProductos.SingleAsync();
        Assert.Equal(8m, storedStock.StockActual);
        Assert.Equal(100m, storedStock.CostoPromedio);

        var costEntry = await context.AsientosContables.Include(x => x.Detalles).SingleAsync(x => x.Id == created.AsientoCostoVentaId);
        Assert.Equal(200m, costEntry.Detalles.Single(x => x.CuentaContableId == cost.Id).Debe);
        Assert.Equal(200m, costEntry.Detalles.Single(x => x.CuentaContableId == inventory.Id).Haber);
    }

    [Fact]
    public async Task CrearVentaConProducto_SinStockNoGuardaVenta()
    {
        await using var context = CreateContext();
        var company = Empresa.Crear("Empresa de prueba", "20888888888");
        context.Empresas.Add(company);
        await context.SaveChangesAsync();
        var cash = CuentaContable.CrearParaEmpresa(company.Id, "10", "Efectivo", "ACTIVO");
        var tax = CuentaContable.CrearParaEmpresa(company.Id, "4011", "IGV", "PASIVO");
        var sales = CuentaContable.CrearParaEmpresa(company.Id, "70", "Ventas", "INGRESO");
        var inventory = CuentaContable.CrearParaEmpresa(company.Id, "20", "Mercaderías", "ACTIVO");
        var cost = CuentaContable.CrearParaEmpresa(company.Id, "69", "Costo de ventas", "GASTO");
        var retained = CuentaContable.CrearParaEmpresa(company.Id, "59", "Resultados acumulados", "PATRIMONIO");
        var unit = UnidadMedida.Crear(company.Id, "UND", "Unidad", "UND");
        var taxDefinition = Impuesto.Crear(company.Id, "IGV", "IGV", 18m);
        var customer = Cliente.Crear(company.Id, "10333333333", "Cliente de prueba");
        context.CuentasContables.AddRange(cash, tax, sales, inventory, cost, retained);
        context.UnidadesMedida.Add(unit); context.Impuestos.Add(taxDefinition); context.Clientes.Add(customer);
        await context.SaveChangesAsync();
        context.ConfiguracionesContablesEmpresas.Add(ConfiguracionContableEmpresa.Crear(company.Id, retained.Id, inventory.Id, cost.Id));
        await context.SaveChangesAsync();
        var product = ProductoServicio.Crear(company.Id, "PRODUCTO", "PROD-002", "Producto sin stock", null, null, unit.Id, taxDefinition.Id, 150m, 100m);
        context.ProductosServicios.Add(product);
        await context.SaveChangesAsync();

        var service = new VentasService(context, new CierreContableService(context), new InventarioService(context, new CierreContableService(context)));
        await Assert.ThrowsAsync<ArgumentException>(() => service.CrearVentaAsync(company.Id, new VentaCreateDto(
            DateTime.Today, customer.Id, "FACTURA", "F001", "00000002", 0m, "CONTADO", cash.Id, tax.Id,
            [new VentaDetalleCreateDto(sales.Id, "Producto sin stock", 1m, 150m, product.Id)])));

        Assert.Empty(await context.Ventas.ToListAsync());
        Assert.Empty(await context.AsientosContables.ToListAsync());
    }

    private static ERPDbContext CreateContext()
        => new(new DbContextOptionsBuilder<ERPDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
}
