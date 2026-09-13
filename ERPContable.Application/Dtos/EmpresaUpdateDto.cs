namespace ERPContable.Application.Dtos;

public sealed record EmpresaUpdateDto(
    string RazonSocial,
    string DocumentoIdentidad,
    string? NombreComercial,
    string? Direccion,
    string? Telefono,
    string? Email,
    bool Activa,
    string? EstadoSunat = null,
    string? CondicionSunat = null);
