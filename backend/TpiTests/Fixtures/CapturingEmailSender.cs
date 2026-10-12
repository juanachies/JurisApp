using System.Collections.Concurrent;
using JurisApp.Application.Interfaces.Auth;

namespace JurisApp.TpiTests.Fixtures;

public sealed class CapturingEmailSender : IEmailSender
{
    public ConcurrentQueue<string> ResetLinks { get; } = new();
    public ConcurrentQueue<string> VerificationCodes { get; } = new();

    public string? LastResetLink => ResetLinks.LastOrDefault();
    public string? LastVerificationCode => VerificationCodes.LastOrDefault();

    public Task SendPasswordResetEmailAsync(string email, string resetLink, CancellationToken cancellationToken = default)
    {
        ResetLinks.Enqueue(resetLink);
        return Task.CompletedTask;
    }

    public Task SendEmailVerificationCodeAsync(string email, string verificationCode, CancellationToken cancellationToken = default)
    {
        VerificationCodes.Enqueue(verificationCode);
        return Task.CompletedTask;
    }
}
