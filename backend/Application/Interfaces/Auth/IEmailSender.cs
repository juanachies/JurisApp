namespace JurisApp.Application.Interfaces.Auth;

public interface IEmailSender
{
    Task SendPasswordResetEmailAsync(string email, string resetLink, CancellationToken cancellationToken = default);
    Task SendEmailVerificationCodeAsync(string email, string verificationCode, CancellationToken cancellationToken = default);
}
