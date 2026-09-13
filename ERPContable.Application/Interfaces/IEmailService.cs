namespace ERPContable.Application.Interfaces;

public interface IEmailService
{
    Task EnviarAsync(string destinatario, string asunto, string contenidoHtml, CancellationToken cancellationToken = default);
}
