using ERPContable.Domain.Entities;
using ERPContable.Persistence.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ERPContable.API.Services;

/// <summary>
/// Datos de demostración únicamente para el entorno Development.
/// Se identifica por el RUC demo y puede ejecutarse varias veces sin duplicar registros.
/// </summary>
public static class DemoDataSeeder
{
    private const string DemoRuc = "20609988776";

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ERPDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DemoDataSeeder");

        var user = await userManager.Users.OrderBy(x => x.CreadoEnUtc).FirstOrDefaultAsync(cancellationToken);
        if (user is null)
        {
            logger.LogInformation("Datos demo omitidos: registra primero un usuario.");
            return;
        }

        var existingCompany = await db.Empresas.FirstOrDefaultAsync(x => x.DocumentoIdentidad == DemoRuc, cancellationToken);
        if (existingCompany is not null)
        {
            if (!await db.UsuariosEmpresas.AnyAsync(x => x.EmpresaId == existingCompany.Id && x.UsuarioId == user.Id, cancellationToken))
            {
                db.UsuariosEmpresas.Add(new UsuarioEmpresa { UsuarioId = user.Id, EmpresaId = existingCompany.Id, Rol = RolesERP.Administrador, Activo = true });
                await db.SaveChangesAsync(cancellationToken);
            }

            await SeedCatalogDefaultsAsync(db, existingCompany.Id, cancellationToken);

            logger.LogInformation("Datos demo ya existentes para {Ruc}.", DemoRuc);
            return;
        }

        var company = Empresa.Crear(
            "Comercial Andina S.A.C.", DemoRuc, "Comercial Andina",
            "Av. Javier Prado Este 1234, San Isidro, Lima", "(01) 555-0147",
            "contacto@comercialandina.demo", "ACTIVO", "HABIDO");
        db.Empresas.Add(company);
        await db.SaveChangesAsync(cancellationToken);

        db.UsuariosEmpresas.Add(new UsuarioEmpresa { UsuarioId = user.Id, EmpresaId = company.Id, Rol = RolesERP.Administrador, Activo = true });

        var accounts = PlanContableBase.Cuentas
            .Select(x => CuentaContable.CrearParaEmpresa(company.Id, x.Codigo, x.Nombre, x.Tipo))
            .ToList();
        db.CuentasContables.AddRange(accounts);
        await db.SaveChangesAsync(cancellationToken);

        var accountByCode = accounts.ToDictionary(x => x.Codigo, StringComparer.OrdinalIgnoreCase);
        int Account(string code) => accountByCode[code].Id;

        db.ConfiguracionesContablesEmpresas.Add(ConfiguracionContableEmpresa.Crear(company.Id, Account("59"), Account("20"), Account("69")));

        var supplier = Proveedor.Crear(company.Id, "20123456789", "Distribuidora Lima Norte S.A.C.", "Jr. Los Olivos 456, Lima", "ventas@distribuidoralima.demo");
        var customer = Cliente.Crear(company.Id, "10456789123", "Restaurante El Buen Sabor E.I.R.L.", "Calle Las Flores 789, Miraflores", "compras@elbuensabor.demo");
        var secondCustomer = Cliente.Crear(company.Id, "10765432109", "Servicios del Pacífico S.R.L.", "Av. Arequipa 890, Lima", "administracion@serviciospacifico.demo");
        db.Proveedores.Add(supplier);
        db.Clientes.AddRange(customer, secondCustomer);
        await db.SaveChangesAsync(cancellationToken);

        var bank = CuentaBancaria.Crear(company.Id, "Banco Demo Perú", "191-0001234567", "PEN", Account("104"));
        db.CuentasBancarias.Add(bank);
        await db.SaveChangesAsync(cancellationToken);

        var today = DateTime.Today;
        var purchase = Compra.Crear(company.Id, supplier.Id, today.AddDays(-12), "FACTURA", "F001", "00001234", 180m, "CREDITO", Account("42"), Account("4011"),
            [CompraDetalle.Crear(Account("60"), "Mercadería para venta", 1m, 1000m)]);
        var sale = Venta.Crear(company.Id, customer.Id, today.AddDays(-8), "FACTURA", "F001", "00000421", 270m, "CONTADO", Account("10"), Account("4011"),
            [VentaDetalle.Crear(Account("70"), "Venta de productos", 1m, 1500m)]);
        var secondSale = Venta.Crear(company.Id, secondCustomer.Id, today.AddDays(-3), "BOLETA", "B001", "00000817", 90m, "CREDITO", Account("12"), Account("4011"),
            [VentaDetalle.Crear(Account("70"), "Servicio de consultoría", 1m, 500m)]);

        var purchaseEntry = AsientoContable.Crear(company.Id, purchase.Fecha, "Compra de mercadería F001-00001234", [(Account("60"), 1000m, 0m), (Account("4011"), 0m, 180m), (Account("42"), 0m, 820m)]);
        var saleEntry = AsientoContable.Crear(company.Id, sale.Fecha, "Venta de productos F001-00000421", [(Account("10"), 1770m, 0m), (Account("70"), 0m, 1500m), (Account("4011"), 0m, 270m)]);
        var secondSaleEntry = AsientoContable.Crear(company.Id, secondSale.Fecha, "Venta de servicios B001-00000817", [(Account("12"), 590m, 0m), (Account("70"), 0m, 500m), (Account("4011"), 0m, 90m)]);
        db.Compras.Add(purchase);
        db.Ventas.AddRange(sale, secondSale);
        db.AsientosContables.AddRange(purchaseEntry, saleEntry, secondSaleEntry);
        await db.SaveChangesAsync(cancellationToken);
        purchase.AsignarAsiento(purchaseEntry.Id);
        sale.AsignarAsiento(saleEntry.Id);
        secondSale.AsignarAsiento(secondSaleEntry.Id);

        var cashIn = MovimientoCaja.Crear(company.Id, today.AddDays(-8), "INGRESO", "Cobro factura F001-00000421", 1770m, Account("101"), Account("70"));
        var cashOut = MovimientoCaja.Crear(company.Id, today.AddDays(-5), "EGRESO", "Pago de servicio de internet", 236m, Account("101"), Account("63"));
        var bankIn = MovimientoBanco.Crear(company.Id, bank.Id, today.AddDays(-2), "INGRESO", "Transferencia de cliente", 590m, Account("12"));
        db.MovimientosCaja.AddRange(cashIn, cashOut);
        db.MovimientosBanco.Add(bankIn);
        await db.SaveChangesAsync(cancellationToken);

        var cashInEntry = AsientoContable.Crear(company.Id, cashIn.Fecha, cashIn.Descripcion, [(Account("101"), 1770m, 0m), (Account("12"), 0m, 1770m)]);
        var cashOutEntry = AsientoContable.Crear(company.Id, cashOut.Fecha, cashOut.Descripcion, [(Account("63"), 236m, 0m), (Account("101"), 0m, 236m)]);
        var bankInEntry = AsientoContable.Crear(company.Id, bankIn.Fecha, bankIn.Descripcion, [(Account("104"), 590m, 0m), (Account("12"), 0m, 590m)]);
        db.AsientosContables.AddRange(cashInEntry, cashOutEntry, bankInEntry);
        await db.SaveChangesAsync(cancellationToken);
        cashIn.AsignarAsiento(cashInEntry.Id);
        cashOut.AsignarAsiento(cashOutEntry.Id);
        bankIn.AsignarAsiento(bankInEntry.Id);

        db.ActivosFijos.Add(ActivoFijo.Crear(company.Id, "AF-001", "Laptop para administración", today.AddMonths(-4), 3600m, 36, Account("33"), Account("39"), Account("68")));
        await db.SaveChangesAsync(cancellationToken);
        await SeedCatalogDefaultsAsync(db, company.Id, cancellationToken);

        logger.LogInformation("Datos demo creados para {Empresa} y usuario {Correo}.", company.RazonSocial, user.Email);
    }

    private static async Task SeedCatalogDefaultsAsync(ERPDbContext db, int empresaId, CancellationToken cancellationToken)
    {
        if (!await db.UnidadesMedida.AnyAsync(x => x.EmpresaId == empresaId, cancellationToken))
        {
            db.UnidadesMedida.AddRange(
                UnidadMedida.Crear(empresaId, "UND", "Unidad", "und"),
                UnidadMedida.Crear(empresaId, "SER", "Servicio", "srv"));
        }
        if (!await db.Impuestos.AnyAsync(x => x.EmpresaId == empresaId, cancellationToken))
            db.Impuestos.Add(Impuesto.Crear(empresaId, "IGV18", "IGV general", 18m));
        if (!await db.CategoriasProductos.AnyAsync(x => x.EmpresaId == empresaId, cancellationToken))
            db.CategoriasProductos.AddRange(CategoriaProducto.Crear(empresaId, "Mercadería", "Productos para venta"), CategoriaProducto.Crear(empresaId, "Servicios", "Servicios prestados"));
        await db.SaveChangesAsync(cancellationToken);

        if (!await db.ProductosServicios.AnyAsync(x => x.EmpresaId == empresaId, cancellationToken))
        {
            var unit = await db.UnidadesMedida.FirstAsync(x => x.EmpresaId == empresaId && x.Codigo == "UND", cancellationToken);
            var tax = await db.Impuestos.FirstAsync(x => x.EmpresaId == empresaId && x.Codigo == "IGV18", cancellationToken);
            var category = await db.CategoriasProductos.FirstAsync(x => x.EmpresaId == empresaId && x.Nombre == "Mercadería", cancellationToken);
            db.ProductosServicios.Add(ProductoServicio.Crear(empresaId, "PRODUCTO", "PROD-001", "Kit de oficina", "Producto demo para ventas", category.Id, unit.Id, tax.Id, 150m, 100m));
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
