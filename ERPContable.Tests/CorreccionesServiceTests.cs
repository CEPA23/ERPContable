using ERPContable.Application.Dtos;
using ERPContable.Domain.Entities;
using ERPContable.Infrastructure.Services;
using ERPContable.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ERPContable.Tests;

// Set ERPCONTABLE_TEST_CONNECTION to run the same scenarios against an isolated PostgreSQL database.
public sealed class CorreccionesDatabase : IAsyncLifetime
{
    private readonly string? source = Environment.GetEnvironmentVariable("ERPCONTABLE_TEST_CONNECTION");
    private readonly string databaseName = "erp_correcciones_test_" + Guid.NewGuid().ToString("N");
    private string? connection;
    private bool created;
    public bool Relational => connection is not null;
    public ERPDbContext Context() => new(connection is null
        ? new DbContextOptionsBuilder<ERPDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options
        : new DbContextOptionsBuilder<ERPDbContext>().UseNpgsql(connection, x => x.MigrationsHistoryTable("__EFMigrationsHistory", "erp")).Options);
    public async Task InitializeAsync()
    {
        if (string.IsNullOrWhiteSpace(source)) return;
        var builder = new NpgsqlConnectionStringBuilder(source) { Database = "postgres", Pooling = false };
        await using var admin = new NpgsqlConnection(builder.ConnectionString);
        await admin.OpenAsync();
        await using var create = new NpgsqlCommand($"CREATE DATABASE \"{databaseName}\"", admin);
        await create.ExecuteNonQueryAsync();
        created = true;
        builder.Database = databaseName;
        connection = builder.ConnectionString;
        await using var db = Context();
        await db.Database.MigrateAsync();
    }
    public async Task DisposeAsync()
    {
        if (!created || source is null || !databaseName.StartsWith("erp_correcciones_test_", StringComparison.Ordinal)) return;
        var builder = new NpgsqlConnectionStringBuilder(source) { Database = "postgres", Pooling = false };
        await using var admin = new NpgsqlConnection(builder.ConnectionString);
        await admin.OpenAsync();
        await using var drop = new NpgsqlCommand($"DROP DATABASE \"{databaseName}\" WITH (FORCE)", admin);
        await drop.ExecuteNonQueryAsync();
    }
}

public sealed class CorreccionesServiceTests(CorreccionesDatabase database) : IClassFixture<CorreccionesDatabase>
{
    private static readonly DateTime Fecha = new(2026, 9, 5);

    private sealed class Scenario(ERPDbContext db, int empresa, int producto, int cash, int tax, int income, int stock, int cost, int retained, int customer, int supplier) : IAsyncDisposable
    {
        public ERPDbContext Db => db;
        public int Empresa => empresa;
        public int Producto => producto;
        public int Inventario => stock;
        public int Costo => cost;
        public int Patrimonio => retained;
        public CorreccionesService Service => new(db, new CierreContableService(db));
        public InventarioService Inventory => new(db, new CierreContableService(db));
        public async Task<Venta> Sell(decimal qty = 3, decimal price = 150, decimal igv = 81)
        {
            var created = await new VentasService(db, new CierreContableService(db), Inventory).CrearVentaAsync(empresa,
                new VentaCreateDto(Fecha, customer, "FACTURA", "T001", Guid.NewGuid().ToString("N")[..12], igv, "CONTADO", cash, tax,
                    [new VentaDetalleCreateDto(income, "Producto de prueba", qty, price, producto)]));
            return await db.Ventas.Include(x => x.Detalles).SingleAsync(x => x.Id == created.Id);
        }
        public async Task<Compra> Buy(decimal qty = 5, decimal price = 100, decimal igv = 90)
        {
            var created = await new ComprasService(db, new CierreContableService(db), Inventory).CrearCompraAsync(empresa,
                new CompraCreateDto(Fecha, supplier, "FACTURA", "T001", Guid.NewGuid().ToString("N")[..12], igv, "CONTADO", cash, tax,
                    [new CompraDetalleCreateDto(stock, "Producto de prueba", qty, price, producto)]));
            return await db.Compras.Include(x => x.Detalles).SingleAsync(x => x.Id == created.Id);
        }
        public Task<CorreccionesDocumentoDto> Correct(bool venta, int id, string tipo, params CorreccionLineaRequest[] lines)
            => Service.RegistrarAsync(empresa, venta, id, Request(tipo, lines), "test-user", "Contador de prueba");
        public Task<InventarioProducto> Stock() => db.InventarioProductos.SingleAsync(x => x.EmpresaId == empresa && x.ProductoServicioId == producto);
        public async ValueTask DisposeAsync() => await db.DisposeAsync();
    }

    private async Task<Scenario> Setup()
    {
        var db = database.Context();
        var company = Empresa.Crear("Pruebas de correcciones", "20" + Random.Shared.NextInt64(100000000, 999999999));
        db.Empresas.Add(company); await db.SaveChangesAsync();
        var accounts = new[] { ("10", "ACTIVO"), ("40", "PASIVO"), ("70", "INGRESO"), ("20", "ACTIVO"), ("69", "GASTO"), ("59", "PATRIMONIO") }
            .Select(x => CuentaContable.CrearParaEmpresa(company.Id, x.Item1, x.Item1, x.Item2)).ToArray();
        var unit = UnidadMedida.Crear(company.Id, "UND", "Unidad", "UND");
        var tax = Impuesto.Crear(company.Id, "IGV", "IGV", 18);
        var customer = Cliente.Crear(company.Id, "10000000001", "Cliente");
        var supplier = Proveedor.Crear(company.Id, "20000000001", "Proveedor");
        db.AddRange(accounts); db.AddRange(unit, tax, customer, supplier); await db.SaveChangesAsync();
        var product = ProductoServicio.Crear(company.Id, "PRODUCTO", "P001", "Producto de prueba", null, null, unit.Id, tax.Id, 150, 100);
        db.Add(product); db.Add(ConfiguracionContableEmpresa.Crear(company.Id, accounts[5].Id, accounts[3].Id, accounts[4].Id)); await db.SaveChangesAsync();
        var stock = InventarioProducto.Crear(company.Id, product.Id); stock.AplicarMovimiento(10, 100); db.Add(stock); await db.SaveChangesAsync();
        return new(db, company.Id, product.Id, accounts[0].Id, accounts[1].Id, accounts[2].Id, accounts[3].Id, accounts[4].Id, accounts[5].Id, customer.Id, supplier.Id);
    }

    private static CorreccionRequest Request(string tipo, params CorreccionLineaRequest[] lines)
        => new(Guid.NewGuid(), tipo, Fecha, "Pedido incorrecto", lines);

    [Fact]
    public async Task DevolucionesParcialesUsanCostoYCuentasOriginalesYCompletanElDocumento()
    {
        await using var s = await Setup();
        var sale = await s.Sell();
        await s.Inventory.RegistrarMovimientoAsync(s.Empresa, new(s.Producto, "ENTRADA", 7, null, 200, "Otro lote", null, Fecha));
        var alternate = CuentaContable.CrearParaEmpresa(s.Empresa, "201", "Otro inventario", "ACTIVO");
        s.Db.Add(alternate); await s.Db.SaveChangesAsync();
        (await s.Db.ConfiguracionesContablesEmpresas.SingleAsync(x => x.EmpresaId == s.Empresa)).Actualizar(s.Patrimonio, alternate.Id, s.Costo);
        await s.Db.SaveChangesAsync();
        var first = await s.Correct(true, sale.Id, "DEVOLUCION", new CorreccionLineaRequest(sale.Detalles[0].Id, 1));
        Assert.Equal("DEVOLUCION_PARCIAL", first.Estado);
        Assert.Equal(177, first.TotalRevertido);
        var costEntry = await s.Db.AsientosContables.Include(x => x.Detalles).SingleAsync(x => x.Id == first.Historial[0].AsientoCostoId);
        Assert.Equal(100, costEntry.Detalles.Single(x => x.CuentaContableId == s.Inventario).Debe);
        Assert.Equal(100, costEntry.Detalles.Single(x => x.CuentaContableId == s.Costo).Haber);
        var complete = await s.Correct(true, sale.Id, "DEVOLUCION", new CorreccionLineaRequest(sale.Detalles[0].Id, 2));
        Assert.Equal("DEVUELTO", complete.Estado);
        Assert.Equal(sale.Total, complete.TotalRevertido);
        Assert.Equal(17, (await s.Stock()).StockActual);
        Assert.Equal(300, complete.Historial.SelectMany(x => x.Detalles).Sum(x => x.CostoInventario));
        Assert.All(complete.Historial, h => Assert.Equal("Contador de prueba", h.Usuario));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task AnulacionRevierteAsientosStockYNoSeRepite(bool venta)
    {
        await using var s = await Setup();
        var id = venta ? (await s.Sell()).Id : (await s.Buy()).Id;
        var result = await s.Correct(venta, id, "ANULACION");
        Assert.Equal("ANULADO", result.Estado);
        Assert.Equal(result.TotalOriginal, result.TotalRevertido);
        Assert.Equal(10, (await s.Stock()).StockActual);
        var balances = await s.Db.DetallesAsiento.Where(x => x.CuentaContable!.EmpresaId == s.Empresa).ToListAsync();
        Assert.All(balances.GroupBy(x => x.CuentaContableId), g => Assert.Equal(g.Sum(x => x.Debe), g.Sum(x => x.Haber)));
        await Assert.ThrowsAsync<ArgumentException>(() => s.Correct(venta, id, "ANULACION"));
        Assert.Single(await s.Db.CorreccionesDocumentos.Where(x => x.EmpresaId == s.Empresa).ToListAsync());
    }

    [Fact]
    public async Task ReintentarLaMismaSolicitudNoDuplicaStockNiAsientos()
    {
        await using var s = await Setup();
        var sale = await s.Sell(); var request = Request("ANULACION");
        var a = await s.Service.RegistrarAsync(s.Empresa, true, sale.Id, request, "u", "Usuario");
        var b = await s.Service.RegistrarAsync(s.Empresa, true, sale.Id, request, "u", "Usuario");
        Assert.Equal(a.Historial[0].Id, b.Historial[0].Id);
        Assert.Equal(10, (await s.Stock()).StockActual);
        Assert.Single(b.Historial);
    }

    [Fact]
    public async Task CompraSinStockSeRechazaSinGuardarCorrecciones()
    {
        await using var s = await Setup(); var buy = await s.Buy();
        await s.Inventory.RegistrarMovimientoAsync(s.Empresa, new(s.Producto, "SALIDA", 14, null, 0, null, null, Fecha));
        await Assert.ThrowsAsync<ArgumentException>(() => s.Correct(false, buy.Id, "ANULACION"));
        Assert.Empty(await s.Db.CorreccionesDocumentos.Where(x => x.EmpresaId == s.Empresa).ToListAsync());
        Assert.Equal(1, (await s.Stock()).StockActual);
    }

    [Fact]
    public async Task CompraConDistintoCostoRecalculaElPromedioRestante()
    {
        await using var s = await Setup(); var buy = await s.Buy(10, 200, 0);
        Assert.Equal(150, (await s.Stock()).CostoPromedio);
        await s.Correct(false, buy.Id, "DEVOLUCION", new CorreccionLineaRequest(buy.Detalles[0].Id, 10));
        Assert.Equal(10, (await s.Stock()).StockActual);
        Assert.Equal(100, (await s.Stock()).CostoPromedio);
    }

    [Fact]
    public async Task VariasDevolucionesConRedondeosSumanExactamenteElOriginal()
    {
        await using var s = await Setup(); var sale = await s.Sell(3, 0.3333m, 0.19m);
        for (var i = 0; i < 3; i++) await s.Correct(true, sale.Id, "DEVOLUCION", new CorreccionLineaRequest(sale.Detalles[0].Id, 1));
        var result = await s.Service.ObtenerAsync(s.Empresa, true, sale.Id);
        Assert.Equal(sale.Subtotal, result.Historial.Sum(x => x.Subtotal));
        Assert.Equal(sale.Igv, result.Historial.Sum(x => x.Igv));
        Assert.Equal(sale.Total, result.TotalRevertido);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(4)]
    [InlineData(0.00001)]
    public async Task RechazaCantidadesInvalidas(decimal qty)
    {
        await using var s = await Setup(); var sale = await s.Sell();
        await Assert.ThrowsAsync<ArgumentException>(() => s.Correct(true, sale.Id, "DEVOLUCION", new CorreccionLineaRequest(sale.Detalles[0].Id, qty)));
        Assert.Empty((await s.Service.ObtenerAsync(s.Empresa, true, sale.Id)).Historial);
    }

    [Fact]
    public async Task RechazaPeriodoCerradoYPermiteDevolucionEnPeriodoPosteriorAbierto()
    {
        await using var s = await Setup(); var sale = await s.Sell();
        s.Db.Add(CierrePeriodo.Cerrar(s.Empresa, 2026, 9, "u", null)); await s.Db.SaveChangesAsync();
        await Assert.ThrowsAsync<ArgumentException>(() => s.Correct(true, sale.Id, "ANULACION"));
        await Assert.ThrowsAsync<ArgumentException>(() => s.Correct(true, sale.Id, "DEVOLUCION", new CorreccionLineaRequest(sale.Detalles[0].Id, 1)));
        var later = Request("DEVOLUCION", new CorreccionLineaRequest(sale.Detalles[0].Id, 1)) with { Fecha = new DateTime(2026, 10, 1) };
        var result = await s.Service.RegistrarAsync(s.Empresa, true, sale.Id, later, "u", "Usuario");
        Assert.Single(result.Historial);
    }

    [Fact]
    public async Task RechazaDocumentoDeOtraEmpresaYLineasRepetidas()
    {
        await using var s = await Setup(); var sale = await s.Sell();
        var other = Empresa.Crear("Otra empresa", "20111111111"); s.Db.Add(other); await s.Db.SaveChangesAsync();
        await Assert.ThrowsAsync<ArgumentException>(() => s.Service.RegistrarAsync(other.Id, true, sale.Id, Request("ANULACION"), "u", "Usuario"));
        await Assert.ThrowsAsync<ArgumentException>(() => s.Correct(true, sale.Id, "DEVOLUCION", new CorreccionLineaRequest(sale.Detalles[0].Id, 1), new CorreccionLineaRequest(sale.Detalles[0].Id, 1)));
    }

    [Fact]
    public async Task DevolucionParcialImpideAnularYExigeDevolverElResto()
    {
        await using var s = await Setup(); var sale = await s.Sell();
        await s.Correct(true, sale.Id, "DEVOLUCION", new CorreccionLineaRequest(sale.Detalles[0].Id, 1));
        await Assert.ThrowsAsync<ArgumentException>(() => s.Correct(true, sale.Id, "ANULACION"));
        await Assert.ThrowsAsync<ArgumentException>(() => s.Correct(true, sale.Id, "DEVOLUCION", new CorreccionLineaRequest(sale.Detalles[0].Id, 3)));
    }

    [Fact]
    public async Task RechazaMotivoVacioFechaAnteriorYLineasAjenas()
    {
        await using var s = await Setup(); var sale = await s.Sell();
        await Assert.ThrowsAsync<ArgumentException>(() => s.Service.RegistrarAsync(s.Empresa, true, sale.Id, Request("ANULACION") with { Motivo = " " }, "u", "U"));
        await Assert.ThrowsAsync<ArgumentException>(() => s.Service.RegistrarAsync(s.Empresa, true, sale.Id, Request("ANULACION") with { Fecha = Fecha.AddDays(-1) }, "u", "U"));
        await Assert.ThrowsAsync<ArgumentException>(() => s.Correct(true, sale.Id, "DEVOLUCION", new CorreccionLineaRequest(int.MaxValue, 1)));
    }
}
