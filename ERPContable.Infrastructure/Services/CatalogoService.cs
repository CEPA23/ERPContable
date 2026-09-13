using ERPContable.Application.Dtos;
using ERPContable.Application.Interfaces;
using ERPContable.Domain.Entities;
using ERPContable.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPContable.Infrastructure.Services;

public sealed class CatalogoService : ICatalogoService
{
    private readonly ERPDbContext _db;
    public CatalogoService(ERPDbContext db) => _db = db;

    public async Task<CatalogoDto> ObtenerAsync(int empresaId, CancellationToken ct = default)
    {
        var productos = await _db.ProductosServicios.AsNoTracking().Include(x => x.Categoria).Include(x => x.UnidadMedida).Include(x => x.Impuesto).Where(x => x.EmpresaId == empresaId).OrderBy(x => x.Nombre).ToListAsync(ct);
        var categorias = await _db.CategoriasProductos.AsNoTracking().Where(x => x.EmpresaId == empresaId).OrderBy(x => x.Nombre).ToListAsync(ct);
        var unidades = await _db.UnidadesMedida.AsNoTracking().Where(x => x.EmpresaId == empresaId).OrderBy(x => x.Nombre).ToListAsync(ct);
        var impuestos = await _db.Impuestos.AsNoTracking().Where(x => x.EmpresaId == empresaId).OrderBy(x => x.Codigo).ToListAsync(ct);
        var centros = await _db.CentrosCosto.AsNoTracking().Where(x => x.EmpresaId == empresaId).OrderBy(x => x.Codigo).ToListAsync(ct);
        return new CatalogoDto(productos.Select(Map).ToList(), categorias.Select(Map).ToList(), unidades.Select(Map).ToList(), impuestos.Select(Map).ToList(), centros.Select(Map).ToList());
    }

    public async Task<CategoriaDto> CrearCategoriaAsync(int empresaId, CategoriaCreateDto request, CancellationToken ct = default)
    {
        await EnsureCompany(empresaId, ct); if (await _db.CategoriasProductos.AnyAsync(x => x.EmpresaId == empresaId && x.Nombre == request.Nombre.Trim(), ct)) throw new ArgumentException("Ya existe una categoría con ese nombre.");
        var item = CategoriaProducto.Crear(empresaId, request.Nombre, request.Descripcion); _db.CategoriasProductos.Add(item); await _db.SaveChangesAsync(ct); return Map(item);
    }

    public async Task<UnidadMedidaDto> CrearUnidadAsync(int empresaId, UnidadMedidaCreateDto request, CancellationToken ct = default)
    {
        await EnsureCompany(empresaId, ct); if (await _db.UnidadesMedida.AnyAsync(x => x.EmpresaId == empresaId && x.Codigo == request.Codigo.Trim().ToUpper(), ct)) throw new ArgumentException("Ya existe una unidad con ese código.");
        var item = UnidadMedida.Crear(empresaId, request.Codigo, request.Nombre, request.Abreviatura); _db.UnidadesMedida.Add(item); await _db.SaveChangesAsync(ct); return Map(item);
    }

    public async Task<ImpuestoDto> CrearImpuestoAsync(int empresaId, ImpuestoCreateDto request, CancellationToken ct = default)
    {
        await EnsureCompany(empresaId, ct); if (await _db.Impuestos.AnyAsync(x => x.EmpresaId == empresaId && x.Codigo == request.Codigo.Trim().ToUpper(), ct)) throw new ArgumentException("Ya existe un impuesto con ese código.");
        var item = Impuesto.Crear(empresaId, request.Codigo, request.Nombre, request.Tasa); _db.Impuestos.Add(item); await _db.SaveChangesAsync(ct); return Map(item);
    }

    public async Task<ProductoDto> CrearProductoAsync(int empresaId, ProductoCreateDto request, CancellationToken ct = default)
    {
        await EnsureCompany(empresaId, ct); var codigo = request.Codigo.Trim().ToUpperInvariant();
        if (await _db.ProductosServicios.AnyAsync(x => x.EmpresaId == empresaId && x.Codigo == codigo, ct)) throw new ArgumentException("Ya existe un producto o servicio con ese código.");
        if (request.CategoriaId is not null && !await _db.CategoriasProductos.AnyAsync(x => x.Id == request.CategoriaId && x.EmpresaId == empresaId && x.Activa, ct)) throw new ArgumentException("La categoría no pertenece a la empresa o está inactiva.");
        if (!await _db.UnidadesMedida.AnyAsync(x => x.Id == request.UnidadMedidaId && x.EmpresaId == empresaId && x.Activa, ct)) throw new ArgumentException("La unidad de medida no pertenece a la empresa o está inactiva.");
        if (!await _db.Impuestos.AnyAsync(x => x.Id == request.ImpuestoId && x.EmpresaId == empresaId && x.Activa, ct)) throw new ArgumentException("El impuesto no pertenece a la empresa o está inactivo.");
        var item = ProductoServicio.Crear(empresaId, request.Tipo, codigo, request.Nombre, request.Descripcion, request.CategoriaId, request.UnidadMedidaId, request.ImpuestoId, request.PrecioVenta, request.CostoReferencial); _db.ProductosServicios.Add(item); await _db.SaveChangesAsync(ct); await _db.Entry(item).Reference(x => x.Categoria).LoadAsync(ct); await _db.Entry(item).Reference(x => x.UnidadMedida).LoadAsync(ct); await _db.Entry(item).Reference(x => x.Impuesto).LoadAsync(ct); return Map(item);
    }

    public async Task<CentroCostoDto> CrearCentroCostoAsync(int empresaId, CentroCostoCreateDto request, CancellationToken ct = default)
    {
        await EnsureCompany(empresaId, ct);
        var codigo = request.Codigo.Trim().ToUpperInvariant();
        if (await _db.CentrosCosto.AnyAsync(x => x.EmpresaId == empresaId && x.Codigo == codigo, ct)) throw new ArgumentException("Ya existe un centro de costo con ese código.");
        var item = CentroCosto.Crear(empresaId, codigo, request.Nombre, request.Descripcion);
        _db.CentrosCosto.Add(item); await _db.SaveChangesAsync(ct); return Map(item);
    }

    private async Task EnsureCompany(int empresaId, CancellationToken ct) { if (!await _db.Empresas.AnyAsync(x => x.Id == empresaId && x.Activa, ct)) throw new ArgumentException("La empresa indicada no existe o está inactiva."); }
    private static CategoriaDto Map(CategoriaProducto x) => new(x.Id, x.Nombre, x.Descripcion, x.Activa);
    private static UnidadMedidaDto Map(UnidadMedida x) => new(x.Id, x.Codigo, x.Nombre, x.Abreviatura, x.Activa);
    private static ImpuestoDto Map(Impuesto x) => new(x.Id, x.Codigo, x.Nombre, x.Tasa, x.Activa);
    private static ProductoDto Map(ProductoServicio x) => new(x.Id, x.Tipo, x.Codigo, x.Nombre, x.Descripcion, x.CategoriaId, x.Categoria?.Nombre, x.UnidadMedidaId, $"{x.UnidadMedida?.Nombre} ({x.UnidadMedida?.Abreviatura})", x.ImpuestoId, $"{x.Impuesto?.Codigo} · {x.Impuesto?.Nombre} ({x.Impuesto?.Tasa:0.##}%)", x.PrecioVenta, x.CostoReferencial, x.Activo);
    private static CentroCostoDto Map(CentroCosto x) => new(x.Id, x.Codigo, x.Nombre, x.Descripcion, x.Activo);
}
