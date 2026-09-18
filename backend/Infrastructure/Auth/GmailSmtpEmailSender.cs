using JurisApp.Application.Interfaces.Auth;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace JurisApp.Infrastructure.Auth;

public class GmailSmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GmailSmtpEmailSender> _logger;

    public GmailSmtpEmailSender(IConfiguration configuration, ILogger<GmailSmtpEmailSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendPasswordResetEmailAsync(string email, string resetLink, CancellationToken cancellationToken = default)
    {
        var htmlBody = $@"
            <p>Hacé clic en el siguiente enlace para restablecer tu contraseña:</p>
            <p><a href=""{resetLink}"">{resetLink}</a></p>
            <p>Si no pediste este cambio, podés ignorar este email.</p>";

        await SendAsync(email, "Restablecé tu contraseña", htmlBody, cancellationToken);
    }

    public async Task SendEmailVerificationCodeAsync(string email, string verificationCode, CancellationToken cancellationToken = default)
    {
        var htmlBody = $@"
            <p>Tu código de verificación es:</p>
            <h2>{verificationCode}</h2>
            <p>Ingresalo en la aplicación para confirmar tu email.</p>";

        await SendAsync(email, "Verificá tu email", htmlBody, cancellationToken);
    }

    private async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        var host = _configuration["SMTP_HOST"] ?? _configuration["Email:Smtp:Host"] ?? "smtp.gmail.com";
        var portText = _configuration["SMTP_PORT"] ?? _configuration["Email:Smtp:Port"] ?? "587";
        var username = _configuration["SMTP_USERNAME"] ?? _configuration["Email:Smtp:Username"] ?? string.Empty;
        var password = _configuration["SMTP_PASSWORD"] ?? _configuration["Email:Smtp:Password"] ?? string.Empty;
        var fromAddress = _configuration["EMAIL_FROM"] ?? _configuration["Email:Smtp:From"] ?? username;
        var fromName = _configuration["EMAIL_FROM_NAME"] ?? _configuration["Email:Smtp:FromName"] ?? "JurisApp";

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning(
                "SMTP no configurado. No se envió el email a {Email}. Completar SMTP_USERNAME y SMTP_PASSWORD.",
                toEmail);
            return;
        }

        if (!int.TryParse(portText, out var port))
        {
            _logger.LogWarning("SMTP_PORT inválido ({PortText}). Usando 587 por defecto.", portText);
            port = 587;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromAddress));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html")
        {
            Text = htmlBody
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, SecureSocketOptions.StartTlsWhenAvailable, cancellationToken);

        if (!string.IsNullOrWhiteSpace(username))
        {
            await client.AuthenticateAsync(username, password, cancellationToken);
        }

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

        _logger.LogInformation("Email enviado correctamente por SMTP a {Email}.", toEmail);
    }
}
