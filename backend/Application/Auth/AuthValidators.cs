using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using JurisApp.Application.Common;

namespace JurisApp.Application.Auth;

public static class AuthValidators
{
    public const int MinPasswordLength = 8;

    public static string NormalizeEmail(string email) =>
        email.Trim().ToLowerInvariant();

    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new MailAddress(email);
            return addr.Address.Equals(NormalizeEmail(email), StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    public static bool IsValidPassword(string password) =>
        !string.IsNullOrWhiteSpace(password) && password.Length >= MinPasswordLength;

    public static Error? ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Error.Validation("El email es obligatorio.");

        if (!IsValidEmail(email))
            return Error.Validation("El formato del email no es válido.");

        return null;
    }

    public static Error? ValidatePassword(string password, string fieldName = "contraseña")
    {
        if (string.IsNullOrWhiteSpace(password))
            return Error.Validation($"La {fieldName} es obligatoria.");

        if (!IsValidPassword(password))
            return Error.Validation($"La {fieldName} debe tener al menos {MinPasswordLength} caracteres.");

        return null;
    }

    public static string GenerateSecureToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    public static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }

    public static string GenerateVerificationCode()
    {
        var value = RandomNumberGenerator.GetInt32(0, 1_000_000);
        return value.ToString("D6");
    }

    public static Error? ValidateVerificationCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return Error.Validation("El código es obligatorio.");

        var normalized = code.Trim();
        if (normalized.Length != 6 || !normalized.All(char.IsDigit))
            return Error.Validation("El código debe tener 6 dígitos.");

        return null;
    }
}
