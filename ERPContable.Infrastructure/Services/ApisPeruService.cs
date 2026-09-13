using System.Net;
using System.Text.Json;
using ERPContable.Application.Dtos;
using ERPContable.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ERPContable.Infrastructure.Services;

public sealed class ApisPeruService : IApisPeruService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public ApisPeruService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public Task<ConsultaDocumentoDto?> ConsultarRucAsync(string ruc, CancellationToken cancellationToken = default)
        => ConsultarAsync($"ruc/{ruc}", cancellationToken);

    public Task<ConsultaDocumentoDto?> ConsultarDniAsync(string dni, CancellationToken cancellationToken = default)
        => ConsultarAsync($"dni/{dni}", cancellationToken);

    private async Task<ConsultaDocumentoDto?> ConsultarAsync(string path, CancellationToken cancellationToken)
    {
        var token = _configuration["ApisPeru:Token"];
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("No se configuró ApisPeru:Token.");
        }

        using var response = await _httpClient.GetAsync(
            $"{path}?token={Uri.EscapeDataString(token)}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var detail = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"APIS Perú respondió {(int)response.StatusCode}: {detail}");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = json.RootElement;

        return new ConsultaDocumentoDto(
            GetString(root, "ruc"),
            GetString(root, "dni"),
            GetString(root, "razonSocial"),
            GetString(root, "nombreCompleto"),
            GetString(root, "nombreComercial"),
            GetString(root, "direccion"),
            GetString(root, "estado"),
            GetString(root, "condicion"));
    }

    private static string? GetString(JsonElement root, string name)
        => root.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null
            ? value.ToString()
            : null;
}
