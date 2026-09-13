using System.Net;
using System.Net.Mail;
using ERPContable.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ERPContable.Infrastructure.Services;

public sealed class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task EnviarAsync(string destinatario, string asunto, string contenidoHtml, CancellationToken cancellationToken = default)
    {
        var host = _configuration["Smtp:Host"];
        var remitente = _configuration["Smtp:From"];
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(remitente))
        {
            _logger.LogWarning("SMTP no está configurado. Correo pendiente para {Destinatario}: {Asunto}", destinatario, asunto);
            return;
        }

        using var client = new SmtpClient(host, _configuration.GetValue<int?>("Smtp:Port") ?? 587)
        {
            EnableSsl = _configuration.GetValue("Smtp:EnableSsl", true),
            Credentials = new NetworkCredential(_configuration["Smtp:User"], _configuration["Smtp:Password"])
        };
        using var message = new MailMessage(remitente, destinatario, asunto, contenidoHtml) { IsBodyHtml = true };
        cancellationToken.ThrowIfCancellationRequested();
        await client.SendMailAsync(message, cancellationToken);
    }
}
