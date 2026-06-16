using JurisApp.Application.Interfaces.Auth;
using Microsoft.Extensions.Logging;

namespace JurisApp.Infrastructure.Auth;

public class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendPasswordResetEmailAsync(string email, string resetLink, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Password reset email for {Email}: {ResetLink}",
            email,
            resetLink);

        return Task.CompletedTask;
    }
}
