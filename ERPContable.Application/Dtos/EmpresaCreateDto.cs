namespace ERPContable.Application.Dtos;

public sealed record EmpresaCreateDto(
    string RazonSocial,
    string DocumentoIdentidad,
    string? NombreComercial,
    string? Direccion,
    string? Telefono,
    string? Email,
    string? EstadoSunat = null,
    string? CondicionSunat = null);
