using System.ComponentModel.DataAnnotations;

namespace ERPContable.Domain.Entities;

public class Empresa
{
    private Empresa()
    {
    }

    public int Id { get; private set; }

    [Required]
    [MaxLength(200)]
    public string RazonSocial { get; private set; } = string.Empty;

    [MaxLength(200)]
    public string? NombreComercial { get; private set; }

    [Required]
    [MaxLength(20)]
    public string DocumentoIdentidad { get; private set; } = string.Empty;

    [MaxLength(250)]
    public string? Direccion { get; private set; }

    [MaxLength(30)]
    public string? Telefono { get; private set; }

    [MaxLength(254)]
    public string? Email { get; private set; }

    public bool Activa { get; private set; } = true;

    [MaxLength(80)]
    public string? EstadoSunat { get; private set; }

    [MaxLength(80)]
    public string? CondicionSunat { get; private set; }

    public DateTime CreadaEnUtc { get; private set; }

    public DateTime? ActualizadaEnUtc { get; private set; }

    public ICollection<AsientoContable> AsientosContables { get; private set; } = [];

    public static Empresa Crear(
        string razonSocial,
        string documentoIdentidad,
        string? nombreComercial = null,
        string? direccion = null,
        string? telefono = null,
        string? email = null,
        string? estadoSunat = null,
        string? condicionSunat = null)
    {
        ValidarDatos(razonSocial, documentoIdentidad, nombreComercial, direccion, telefono, email);

        return new Empresa
        {
            RazonSocial = razonSocial.Trim(),
            DocumentoIdentidad = documentoIdentidad.Trim(),
            NombreComercial = NormalizarTexto(nombreComercial),
            Direccion = NormalizarTexto(direccion),
            Telefono = NormalizarTexto(telefono),
            Email = NormalizarTexto(email),
            EstadoSunat = NormalizarTexto(estadoSunat),
            CondicionSunat = NormalizarTexto(condicionSunat),
            Activa = true,
            CreadaEnUtc = DateTime.UtcNow
        };
    }

    public void Actualizar(
        string razonSocial,
        string documentoIdentidad,
        string? nombreComercial = null,
        string? direccion = null,
        string? telefono = null,
        string? email = null,
        bool? activa = null,
        string? estadoSunat = null,
        string? condicionSunat = null)
    {
        ValidarDatos(razonSocial, documentoIdentidad, nombreComercial, direccion, telefono, email);

        RazonSocial = razonSocial.Trim();
        DocumentoIdentidad = documentoIdentidad.Trim();
        NombreComercial = NormalizarTexto(nombreComercial);
        Direccion = NormalizarTexto(direccion);
        Telefono = NormalizarTexto(telefono);
        Email = NormalizarTexto(email);
        EstadoSunat = NormalizarTexto(estadoSunat);
        CondicionSunat = NormalizarTexto(condicionSunat);

        if (activa.HasValue)
        {
            Activa = activa.Value;
        }

        ActualizadaEnUtc = DateTime.UtcNow;
    }

    public void Activar()
    {
        if (!Activa)
        {
            Activa = true;
            ActualizadaEnUtc = DateTime.UtcNow;
        }
    }

    public void Desactivar()
    {
        if (Activa)
        {
            Activa = false;
            ActualizadaEnUtc = DateTime.UtcNow;
        }
    }

    private static void ValidarDatos(
        string razonSocial,
        string documentoIdentidad,
        string? nombreComercial,
        string? direccion,
        string? telefono,
        string? email)
    {
        if (string.IsNullOrWhiteSpace(razonSocial))
        {
            throw new ArgumentException("La razón social es obligatoria.", nameof(razonSocial));
        }

        if (string.IsNullOrWhiteSpace(documentoIdentidad))
        {
            throw new ArgumentException("El documento de identidad es obligatorio.", nameof(documentoIdentidad));
        }

        if (razonSocial.Length > 200)
        {
            throw new ArgumentException("La razón social no puede superar los 200 caracteres.", nameof(razonSocial));
        }

        if (documentoIdentidad.Length > 20)
        {
            throw new ArgumentException("El documento de identidad no puede superar los 20 caracteres.", nameof(documentoIdentidad));
        }

        if (nombreComercial is not null && nombreComercial.Length > 200)
        {
            throw new ArgumentException("El nombre comercial no puede superar los 200 caracteres.", nameof(nombreComercial));
        }

        if (direccion is not null && direccion.Length > 250)
        {
            throw new ArgumentException("La dirección no puede superar los 250 caracteres.", nameof(direccion));
        }

        if (telefono is not null && telefono.Length > 30)
        {
            throw new ArgumentException("El teléfono no puede superar los 30 caracteres.", nameof(telefono));
        }

        if (email is not null && email.Length > 254)
        {
            throw new ArgumentException("El correo no puede superar los 254 caracteres.", nameof(email));
        }

        if (!string.IsNullOrWhiteSpace(email) && new EmailAddressAttribute().IsValid(email) is false)
        {
            throw new ArgumentException("El correo electrónico no tiene un formato válido.", nameof(email));
        }
    }

    private static string? NormalizarTexto(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
