using ERPContable.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ERPContable.Persistence.Data;

public class ERPDbContext : IdentityDbContext<ApplicationUser>
{
    public const string DefaultSchema = "erp";

    public ERPDbContext(DbContextOptions<ERPDbContext> options)
        : base(options)
    {
    }

    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<CuentaContable> CuentasContables => Set<CuentaContable>();
    public DbSet<AsientoContable> AsientosContables => Set<AsientoContable>();
    public DbSet<DetalleAsiento> DetallesAsiento => Set<DetalleAsiento>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Compra> Compras => Set<Compra>();
    public DbSet<CompraDetalle> ComprasDetalle => Set<CompraDetalle>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<VentaDetalle> VentasDetalle => Set<VentaDetalle>();
    public DbSet<MovimientoCaja> MovimientosCaja => Set<MovimientoCaja>();
    public DbSet<CuentaBancaria> CuentasBancarias => Set<CuentaBancaria>();
    public DbSet<MovimientoBanco> MovimientosBanco => Set<MovimientoBanco>();
    public DbSet<ActivoFijo> ActivosFijos => Set<ActivoFijo>();
    public DbSet<AjusteContable> AjustesContables => Set<AjusteContable>();
    public DbSet<UsuarioEmpresa> UsuariosEmpresas => Set<UsuarioEmpresa>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditLog> Auditoria => Set<AuditLog>();
    public DbSet<CierrePeriodo> CierresPeriodos => Set<CierrePeriodo>();
    public DbSet<CierreEjercicio> CierresEjercicios => Set<CierreEjercicio>();
    public DbSet<ConfiguracionContableEmpresa> ConfiguracionesContablesEmpresas => Set<ConfiguracionContableEmpresa>();
    public DbSet<CategoriaProducto> CategoriasProductos => Set<CategoriaProducto>();
    public DbSet<UnidadMedida> UnidadesMedida => Set<UnidadMedida>();
    public DbSet<Impuesto> Impuestos => Set<Impuesto>();
    public DbSet<ProductoServicio> ProductosServicios => Set<ProductoServicio>();
    public DbSet<CentroCosto> CentrosCosto => Set<CentroCosto>();
    public DbSet<InventarioProducto> InventarioProductos => Set<InventarioProducto>();
    public DbSet<MovimientoInventario> MovimientosInventario => Set<MovimientoInventario>();
    public DbSet<CorreccionDocumento> CorreccionesDocumentos => Set<CorreccionDocumento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        if (Database.ProviderName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) == true)
            modelBuilder.HasDefaultSchema(DefaultSchema);
        modelBuilder.Entity<CorreccionDocumento>(entity =>
        {
            entity.ToTable("CorreccionesDocumentos", table => table.HasCheckConstraint("CK_Correccion_Documento", "(\"CompraId\" IS NULL) <> (\"VentaId\" IS NULL)"));
            entity.HasIndex(x => new { x.EmpresaId, x.SolicitudId }).IsUnique();
            entity.Property(x => x.Fecha).HasColumnType("timestamp without time zone");
            entity.Property(x => x.Tipo).HasMaxLength(20);
            entity.Property(x => x.Motivo).HasMaxLength(500);
            entity.Property(x => x.UsuarioId).HasMaxLength(450);
            entity.Property(x => x.UsuarioNombre).HasMaxLength(256);
            entity.Property(x => x.Subtotal).HasPrecision(18, 2);
            entity.Property(x => x.Igv).HasPrecision(18, 2);
            entity.HasOne<Empresa>().WithMany().HasForeignKey(x => x.EmpresaId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Compra>().WithMany(x => x.Correcciones).HasForeignKey(x => x.CompraId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Venta>().WithMany(x => x.Correcciones).HasForeignKey(x => x.VentaId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.AsientoContable).WithMany().HasForeignKey(x => x.AsientoContableId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.AsientoCosto).WithMany().HasForeignKey(x => x.AsientoCostoId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(x => x.Detalles).WithOne().HasForeignKey(x => x.CorreccionDocumentoId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<CorreccionDetalle>(entity =>
        {
            entity.ToTable("CorreccionesDetalles");
            entity.Property(x => x.Cantidad).HasPrecision(18, 4);
            entity.Property(x => x.Subtotal).HasPrecision(18, 2);
            entity.Property(x => x.CostoInventario).HasPrecision(18, 2);
            entity.HasOne<CompraDetalle>().WithMany().HasForeignKey(x => x.CompraDetalleId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<VentaDetalle>().WithMany().HasForeignKey(x => x.VentaDetalleId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.MovimientoInventario).WithMany().HasForeignKey(x => x.MovimientoInventarioId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<VentaDetalle>().Property(x => x.CostoInventario).HasPrecision(18, 2);
        modelBuilder.Entity<VentaDetalle>().Property(x => x.CostoUnitarioInventario).HasPrecision(18, 4);
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Usuarios");
            entity.Property(x => x.NombreCompleto).HasMaxLength(200).IsRequired();
        });
        modelBuilder.Entity<UsuarioEmpresa>(entity =>
        {
            entity.ToTable("UsuariosEmpresas");
            entity.HasKey(x => new { x.UsuarioId, x.EmpresaId });
            entity.Property(x => x.Rol).HasMaxLength(40).IsRequired();
            entity.HasOne(x => x.Usuario).WithMany(x => x.Empresas).HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Empresa).WithMany().HasForeignKey(x => x.EmpresaId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.EmpresaId, x.UsuarioId });
        });
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
            entity.Property(x => x.RolEmpresa).HasMaxLength(40);
            entity.Property(x => x.DireccionIp).HasMaxLength(64);
            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("Auditoria");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Accion).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Entidad).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Detalles).HasColumnType("text");
            entity.Property(x => x.DireccionIp).HasMaxLength(64);
            entity.HasIndex(x => new { x.EmpresaId, x.CreadoEnUtc });
        });
        modelBuilder.Entity<CierrePeriodo>(entity =>
        {
            entity.ToTable("CierresPeriodos");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Observacion).HasMaxLength(500);
            entity.Property(x => x.CerradoPorUsuarioId).HasMaxLength(450);
            entity.Property(x => x.ReabiertoPorUsuarioId).HasMaxLength(450);
            entity.HasIndex(x => new { x.EmpresaId, x.Ejercicio, x.Mes }).IsUnique();
        });
        modelBuilder.Entity<CierreEjercicio>(entity =>
        {
            entity.ToTable("CierresEjercicios");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Observacion).HasMaxLength(500);
            entity.Property(x => x.CerradoPorUsuarioId).HasMaxLength(450);
            entity.Property(x => x.ReabiertoPorUsuarioId).HasMaxLength(450);
            entity.HasIndex(x => new { x.EmpresaId, x.Ejercicio }).IsUnique();
        });
        modelBuilder.Entity<ConfiguracionContableEmpresa>(entity =>
        {
            entity.ToTable("ConfiguracionesContablesEmpresas");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.EmpresaId).IsUnique();
            entity.HasOne<Empresa>().WithMany().HasForeignKey(x => x.EmpresaId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<CuentaContable>().WithMany().HasForeignKey(x => x.CuentaResultadoAcumuladoId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<CuentaContable>().WithMany().HasForeignKey(x => x.CuentaInventarioId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<CuentaContable>().WithMany().HasForeignKey(x => x.CuentaCostoVentasId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<CategoriaProducto>(entity =>
        {
            entity.ToTable("CategoriasProductos");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Nombre).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Descripcion).HasMaxLength(250);
            entity.HasIndex(x => new { x.EmpresaId, x.Nombre }).IsUnique();
            entity.HasOne<Empresa>().WithMany().HasForeignKey(x => x.EmpresaId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<UnidadMedida>(entity =>
        {
            entity.ToTable("UnidadesMedida");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Codigo).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Nombre).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Abreviatura).HasMaxLength(10).IsRequired();
            entity.HasIndex(x => new { x.EmpresaId, x.Codigo }).IsUnique();
            entity.HasOne<Empresa>().WithMany().HasForeignKey(x => x.EmpresaId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Impuesto>(entity =>
        {
            entity.ToTable("Impuestos");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Codigo).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Nombre).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Tasa).HasPrecision(5, 2);
            entity.HasIndex(x => new { x.EmpresaId, x.Codigo }).IsUnique();
            entity.HasOne<Empresa>().WithMany().HasForeignKey(x => x.EmpresaId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<ProductoServicio>(entity =>
        {
            entity.ToTable("ProductosServicios");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Tipo).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Codigo).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Descripcion).HasMaxLength(250);
            entity.Property(x => x.PrecioVenta).HasPrecision(18, 2);
            entity.Property(x => x.CostoReferencial).HasPrecision(18, 2);
            entity.HasIndex(x => new { x.EmpresaId, x.Codigo }).IsUnique();
            entity.HasOne<Empresa>().WithMany().HasForeignKey(x => x.EmpresaId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Categoria).WithMany().HasForeignKey(x => x.CategoriaId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.UnidadMedida).WithMany().HasForeignKey(x => x.UnidadMedidaId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Impuesto).WithMany().HasForeignKey(x => x.ImpuestoId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<CentroCosto>(entity =>
        {
            entity.ToTable("CentrosCosto");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Codigo).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Descripcion).HasMaxLength(250);
            entity.HasIndex(x => new { x.EmpresaId, x.Codigo }).IsUnique();
            entity.HasOne<Empresa>().WithMany().HasForeignKey(x => x.EmpresaId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<InventarioProducto>(entity =>
        {
            entity.ToTable("InventarioProductos");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.StockActual).HasPrecision(18, 4).IsConcurrencyToken();
            entity.Property(x => x.StockMinimo).HasPrecision(18, 4);
            entity.Property(x => x.CostoPromedio).HasPrecision(18, 4).IsConcurrencyToken();
            entity.HasIndex(x => new { x.EmpresaId, x.ProductoServicioId }).IsUnique();
            entity.HasOne<Empresa>().WithMany().HasForeignKey(x => x.EmpresaId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.ProductoServicio).WithMany().HasForeignKey(x => x.ProductoServicioId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<MovimientoInventario>(entity =>
        {
            entity.ToTable("MovimientosInventario");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Tipo).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Cantidad).HasPrecision(18, 4);
            entity.Property(x => x.CostoUnitario).HasPrecision(18, 4);
            entity.Property(x => x.StockAnterior).HasPrecision(18, 4);
            entity.Property(x => x.StockPosterior).HasPrecision(18, 4);
            entity.Property(x => x.Referencia).HasMaxLength(100);
            entity.Property(x => x.Observacion).HasMaxLength(250);
            entity.HasIndex(x => new { x.EmpresaId, x.FechaUtc });
            entity.HasOne<Empresa>().WithMany().HasForeignKey(x => x.EmpresaId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.ProductoServicio).WithMany().HasForeignKey(x => x.ProductoServicioId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<CuentaContable>(entity =>
        {
            entity.HasIndex(x => new { x.EmpresaId, x.Codigo }).IsUnique();
            entity.HasOne<Empresa>().WithMany().HasForeignKey(x => x.EmpresaId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Empresa>().Property(x => x.EstadoSunat).HasMaxLength(80);
        modelBuilder.Entity<Empresa>().Property(x => x.CondicionSunat).HasMaxLength(80);
        modelBuilder.Entity<CuentaContable>().Property(x => x.Codigo).HasMaxLength(20).IsRequired();
        modelBuilder.Entity<CuentaContable>().Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        modelBuilder.Entity<CuentaContable>().Property(x => x.Tipo).HasMaxLength(30).IsRequired();
        modelBuilder.Entity<AsientoContable>().Property(x => x.Glosa).HasMaxLength(250).IsRequired();
        // La fecha del asiento es una fecha contable, no un instante en el tiempo.
        // Se guarda sin zona horaria para evitar conversiones UTC y cambios de día.
        modelBuilder.Entity<AsientoContable>().Property(x => x.Fecha)
            .HasColumnType("timestamp without time zone");
        modelBuilder.Entity<Empresa>().HasMany(x => x.AsientosContables).WithOne(x => x.Empresa).HasForeignKey(x => x.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<AsientoContable>().HasMany(x => x.Detalles).WithOne().HasForeignKey(x => x.AsientoContableId);
        modelBuilder.Entity<DetalleAsiento>().HasOne(x => x.CuentaContable).WithMany().HasForeignKey(x => x.CuentaContableId);
        modelBuilder.Entity<DetalleAsiento>().Property(x => x.Debe).HasPrecision(18, 2);
        modelBuilder.Entity<DetalleAsiento>().Property(x => x.Haber).HasPrecision(18, 2);
        modelBuilder.Entity<Proveedor>().Property(x => x.Documento).HasMaxLength(20).IsRequired();
        modelBuilder.Entity<Proveedor>().Property(x => x.RazonSocial).HasMaxLength(200).IsRequired();
        modelBuilder.Entity<Proveedor>().HasIndex(x => new { x.EmpresaId, x.Documento }).IsUnique();
        modelBuilder.Entity<Empresa>().HasMany<Proveedor>().WithOne(x => x.Empresa).HasForeignKey(x => x.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Compra>().Property(x => x.TipoComprobante).HasMaxLength(20).IsRequired();
        modelBuilder.Entity<Compra>().Property(x => x.Serie).HasMaxLength(10).IsRequired();
        modelBuilder.Entity<Compra>().Property(x => x.Numero).HasMaxLength(20).IsRequired();
        modelBuilder.Entity<Compra>().Property(x => x.FormaPago).HasMaxLength(20).IsRequired();
        modelBuilder.Entity<Compra>().Property(x => x.Fecha).HasColumnType("timestamp without time zone");
        modelBuilder.Entity<Compra>().Property(x => x.Subtotal).HasPrecision(18, 2);
        modelBuilder.Entity<Compra>().Property(x => x.Igv).HasPrecision(18, 2);
        modelBuilder.Entity<Compra>().Property(x => x.Total).HasPrecision(18, 2);
        modelBuilder.Entity<Compra>().HasMany(x => x.Detalles).WithOne().HasForeignKey(x => x.CompraId);
        modelBuilder.Entity<CompraDetalle>().Property(x => x.Cantidad).HasPrecision(18, 4);
        modelBuilder.Entity<CompraDetalle>().Property(x => x.PrecioUnitario).HasPrecision(18, 4);
        modelBuilder.Entity<CompraDetalle>().Property(x => x.Total).HasPrecision(18, 2);
        modelBuilder.Entity<CompraDetalle>().Property(x => x.Descripcion).HasMaxLength(250).IsRequired();
        modelBuilder.Entity<CompraDetalle>().HasOne(x => x.CuentaContable).WithMany().HasForeignKey(x => x.CuentaContableId);
        modelBuilder.Entity<CompraDetalle>().HasOne(x => x.ProductoServicio).WithMany().HasForeignKey(x => x.ProductoServicioId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Cliente>().Property(x => x.Documento).HasMaxLength(20).IsRequired();
        modelBuilder.Entity<Cliente>().Property(x => x.RazonSocial).HasMaxLength(200).IsRequired();
        modelBuilder.Entity<Cliente>().HasIndex(x => new { x.EmpresaId, x.Documento }).IsUnique();
        modelBuilder.Entity<Empresa>().HasMany<Cliente>().WithOne(x => x.Empresa).HasForeignKey(x => x.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Venta>().Property(x => x.TipoComprobante).HasMaxLength(20).IsRequired();
        modelBuilder.Entity<Venta>().Property(x => x.Serie).HasMaxLength(10).IsRequired();
        modelBuilder.Entity<Venta>().Property(x => x.Numero).HasMaxLength(20).IsRequired();
        modelBuilder.Entity<Venta>().Property(x => x.FormaPago).HasMaxLength(20).IsRequired();
        modelBuilder.Entity<Venta>().Property(x => x.Fecha).HasColumnType("timestamp without time zone");
        modelBuilder.Entity<Venta>().Property(x => x.Subtotal).HasPrecision(18, 2);
        modelBuilder.Entity<Venta>().Property(x => x.Igv).HasPrecision(18, 2);
        modelBuilder.Entity<Venta>().Property(x => x.Total).HasPrecision(18, 2);
        modelBuilder.Entity<Venta>().HasMany(x => x.Detalles).WithOne().HasForeignKey(x => x.VentaId);
        modelBuilder.Entity<VentaDetalle>().Property(x => x.Descripcion).HasMaxLength(250).IsRequired();
        modelBuilder.Entity<VentaDetalle>().Property(x => x.Cantidad).HasPrecision(18, 4);
        modelBuilder.Entity<VentaDetalle>().Property(x => x.PrecioUnitario).HasPrecision(18, 4);
        modelBuilder.Entity<VentaDetalle>().Property(x => x.Total).HasPrecision(18, 2);
        modelBuilder.Entity<VentaDetalle>().HasOne(x => x.CuentaContable).WithMany().HasForeignKey(x => x.CuentaContableId);
        modelBuilder.Entity<VentaDetalle>().HasOne(x => x.ProductoServicio).WithMany().HasForeignKey(x => x.ProductoServicioId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<MovimientoCaja>().Property(x => x.Fecha).HasColumnType("timestamp without time zone");
        modelBuilder.Entity<MovimientoCaja>().Property(x => x.Tipo).HasMaxLength(20).IsRequired();
        modelBuilder.Entity<MovimientoCaja>().Property(x => x.Descripcion).HasMaxLength(250).IsRequired();
        modelBuilder.Entity<MovimientoCaja>().Property(x => x.Monto).HasPrecision(18, 2);
        modelBuilder.Entity<CuentaBancaria>().Property(x => x.Banco).HasMaxLength(120).IsRequired();
        modelBuilder.Entity<CuentaBancaria>().Property(x => x.NumeroCuenta).HasMaxLength(50).IsRequired();
        modelBuilder.Entity<CuentaBancaria>().Property(x => x.Moneda).HasMaxLength(3).IsRequired();
        modelBuilder.Entity<CuentaBancaria>().HasIndex(x => new { x.EmpresaId, x.NumeroCuenta }).IsUnique();
        modelBuilder.Entity<Empresa>().HasMany<CuentaBancaria>().WithOne(x => x.Empresa).HasForeignKey(x => x.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<CuentaBancaria>().HasOne(x => x.CuentaContable).WithMany().HasForeignKey(x => x.CuentaContableId);
        modelBuilder.Entity<MovimientoBanco>().Property(x => x.Fecha).HasColumnType("timestamp without time zone");
        modelBuilder.Entity<MovimientoBanco>().Property(x => x.Tipo).HasMaxLength(20).IsRequired();
        modelBuilder.Entity<MovimientoBanco>().Property(x => x.Descripcion).HasMaxLength(250).IsRequired();
        modelBuilder.Entity<MovimientoBanco>().Property(x => x.Monto).HasPrecision(18, 2);
        modelBuilder.Entity<CuentaBancaria>().HasMany<MovimientoBanco>().WithOne(x => x.CuentaBancaria).HasForeignKey(x => x.CuentaBancariaId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<ActivoFijo>().Property(x => x.Codigo).HasMaxLength(30).IsRequired();
        modelBuilder.Entity<ActivoFijo>().Property(x => x.Descripcion).HasMaxLength(250).IsRequired();
        modelBuilder.Entity<ActivoFijo>().Property(x => x.FechaAdquisicion).HasColumnType("timestamp without time zone");
        modelBuilder.Entity<ActivoFijo>().Property(x => x.ValorAdquisicion).HasPrecision(18, 2);
        modelBuilder.Entity<ActivoFijo>().Property(x => x.DepreciacionAcumulada).HasPrecision(18, 2);
        modelBuilder.Entity<ActivoFijo>().HasIndex(x => new { x.EmpresaId, x.Codigo }).IsUnique();
        modelBuilder.Entity<AjusteContable>().Property(x => x.Fecha).HasColumnType("timestamp without time zone");
        modelBuilder.Entity<AjusteContable>().Property(x => x.Glosa).HasMaxLength(250).IsRequired();
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ERPDbContext).Assembly);
    }
}
