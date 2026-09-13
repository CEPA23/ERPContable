namespace ERPContable.Application.Dtos;

public sealed record CategoriaDto(int Id, string Nombre, string? Descripcion, bool Activa);
public sealed record CategoriaCreateDto(string Nombre, string? Descripcion);
public sealed record UnidadMedidaDto(int Id, string Codigo, string Nombre, string Abreviatura, bool Activa);
public sealed record UnidadMedidaCreateDto(string Codigo, string Nombre, string Abreviatura);
public sealed record ImpuestoDto(int Id, string Codigo, string Nombre, decimal Tasa, bool Activa);
public sealed record ImpuestoCreateDto(string Codigo, string Nombre, decimal Tasa);
public sealed record ProductoDto(int Id, string Tipo, string Codigo, string Nombre, string? Descripcion, int? CategoriaId, string? Categoria, int UnidadMedidaId, string UnidadMedida, int ImpuestoId, string Impuesto, decimal PrecioVenta, decimal CostoReferencial, bool Activo);
public sealed record ProductoCreateDto(string Tipo, string Codigo, string Nombre, string? Descripcion, int? CategoriaId, int UnidadMedidaId, int ImpuestoId, decimal PrecioVenta, decimal CostoReferencial);
public sealed record CentroCostoDto(int Id, string Codigo, string Nombre, string? Descripcion, bool Activo);
public sealed record CentroCostoCreateDto(string Codigo, string Nombre, string? Descripcion);
