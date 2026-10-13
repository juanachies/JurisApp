using System.Net.Http.Json;
using System.Text.Json.Serialization;
using JurisApp.Application.Interfaces.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace JurisApp.Infrastructure.Auth;

public class BrevoEmailSender : IEmailSender
{
    private static readonly HttpClient Http = new()
    {
        BaseAddress = new Uri("https://api.brevo.com/")
    };

    private readonly IConfiguration _configuration;
    private readonly ILogger<BrevoEmailSender> _logger;

    public BrevoEmailSender(IConfiguration configuration, ILogger<BrevoEmailSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task SendPasswordResetEmailAsync(string email, string resetLink, CancellationToken cancellationToken = default)
    {
        var htmlBody = $"""
            <p>Hacé clic en el siguiente enlace para restablecer tu contraseña:</p>
            <p><a href="{resetLink}">{resetLink}</a></p>
            <p>Si no pediste este cambio, podés ignorar este email.</p>
            """;

        return SendAsync(email, "Restablecé tu contraseña", htmlBody, cancellationToken);
    }

    public Task SendEmailVerificationCodeAsync(string email, string verificationCode, CancellationToken cancellationToken = default)
    {
        var htmlBody = $"""
            <p>Tu código de verificación es:</p>
            <h2>{verificationCode}</h2>
            <p>Ingresalo en la aplicación para confirmar tu email.</p>
            """;

        return SendAsync(email, "Verificá tu email", htmlBody, cancellationToken);
    }

    private async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        var apiKey = Clean(_configuration["Brevo:ApiKey"]);
        var senderEmail = Clean(_configuration["Brevo:SenderEmail"] ?? _configuration["Email:Smtp:From"]);
        var senderName = Clean(_configuration["Brevo:SenderName"]) is { Length: > 0 } name ? name : "JurisApp";

        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(senderEmail))
            throw new InvalidOperationException("Brevo no está configurado.");

        using var request = new HttpRequestMessage(HttpMethod.Post, "v3/smtp/email");
        request.Headers.TryAddWithoutValidation("api-key", apiKey);
        request.Headers.Accept.ParseAdd("application/json");
        request.Content = JsonContent.Create(new BrevoMessage(
            new BrevoSender(senderName, senderEmail),
            [new BrevoRecipient(toEmail)],
            subject,
            htmlBody));

        using var response = await Http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "Brevo rechazó el email a {Email}. Status {Status}. {Body}",
                toEmail,
                (int)response.StatusCode,
                body);
            throw new InvalidOperationException("No se pudo enviar el email.");
        }

        _logger.LogInformation("Email enviado por Brevo a {Email}.", toEmail);
    }

    private static string Clean(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var trimmed = value.Trim();
        if (trimmed.Length >= 2 && trimmed.StartsWith('"') && trimmed.EndsWith('"'))
            trimmed = trimmed[1..^1].Trim();

        return trimmed;
    }

    private sealed record BrevoMessage(
        [property: JsonPropertyName("sender")] BrevoSender Sender,
        [property: JsonPropertyName("to")] BrevoRecipient[] To,
        [property: JsonPropertyName("subject")] string Subject,
        [property: JsonPropertyName("htmlContent")] string HtmlContent);

    private sealed record BrevoSender(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("email")] string Email);

    private sealed record BrevoRecipient(
        [property: JsonPropertyName("email")] string Email);
}
