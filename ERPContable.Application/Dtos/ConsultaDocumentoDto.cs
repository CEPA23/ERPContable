namespace ERPContable.Application.Dtos;

public sealed record ConsultaDocumentoDto(
    string? Ruc,
    string? Dni,
    string? RazonSocial,
    string? NombreCompleto,
    string? NombreComercial,
    string? Direccion,
    string? Estado = null,
    string? Condicion = null);
