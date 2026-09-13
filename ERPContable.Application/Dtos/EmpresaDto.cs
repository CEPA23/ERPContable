namespace ERPContable.Application.Dtos;

public sealed record EmpresaDto(
    int Id,
    string RazonSocial,
    string DocumentoIdentidad,
    string? NombreComercial,
    string? Direccion,
    string? Telefono,
    string? Email,
    bool Activa,
    DateTime CreadaEnUtc,
    DateTime? ActualizadaEnUtc,
    string? EstadoSunat = null,
    string? CondicionSunat = null);
