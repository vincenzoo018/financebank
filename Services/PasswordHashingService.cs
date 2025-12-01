using BCrypt.Net;

namespace FinanceBank.Services;

/// <summary>
/// Service for secure password hashing and verification using BCrypt
/// </summary>
public class PasswordHashingService
{
    private const int WorkFactor = 12; // BCrypt work factor (higher = more secure but slower)

    /// <summary>
    /// Hash a plain text password using BCrypt
    /// </summary>
    public string HashPassword(string plainTextPassword)
    {
        if (string.IsNullOrWhiteSpace(plainTextPassword))
            throw new ArgumentException("Password cannot be empty", nameof(plainTextPassword));

        return BCrypt.Net.BCrypt.HashPassword(plainTextPassword, WorkFactor);
    }

    /// <summary>
    /// Hash a PIN (shorter than password, no length requirement)
    /// </summary>
    public string HashPin(string pin)
    {
        if (string.IsNullOrWhiteSpace(pin))
            throw new ArgumentException("PIN cannot be empty", nameof(pin));

        return BCrypt.Net.BCrypt.HashPassword(pin, WorkFactor);
    }

    /// <summary>
    /// Verify a plain text password against a BCrypt hash
    /// </summary>
    public bool VerifyPassword(string plainTextPassword, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(plainTextPassword) || string.IsNullOrWhiteSpace(hashedPassword))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(plainTextPassword, hashedPassword);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Check if a password needs to be rehashed (if work factor changed)
    /// </summary>
    public bool NeedsRehash(string hashedPassword)
    {
        try
        {
            return BCrypt.Net.BCrypt.PasswordNeedsRehash(hashedPassword, WorkFactor);
        }
        catch
        {
            return true;
        }
    }

    /// <summary>
    /// Validate password strength requirements
    /// </summary>
    public (bool IsValid, string ErrorMessage) ValidatePasswordStrength(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return (false, "Password cannot be empty");

        if (password.Length < 12)
            return (false, "Password must be at least 12 characters long");

        bool hasUpper = password.Any(char.IsUpper);
        bool hasLower = password.Any(char.IsLower);
        bool hasDigit = password.Any(char.IsDigit);
        bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));

        if (!hasUpper)
            return (false, "Password must contain at least one uppercase letter");

        if (!hasLower)
            return (false, "Password must contain at least one lowercase letter");

        if (!hasDigit)
            return (false, "Password must contain at least one number");

        if (!hasSpecial)
            return (false, "Password must contain at least one special character");

        return (true, string.Empty);
    }

    /// <summary>
    /// Generate a random secure password
    /// </summary>
    public string GenerateSecurePassword(int length = 16)
    {
        const string upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowerChars = "abcdefghijklmnopqrstuvwxyz";
        const string digitChars = "0123456789";
        const string specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";

        var random = new Random();
        var password = new char[length];

        // Ensure at least one of each required type
        password[0] = upperChars[random.Next(upperChars.Length)];
        password[1] = lowerChars[random.Next(lowerChars.Length)];
        password[2] = digitChars[random.Next(digitChars.Length)];
        password[3] = specialChars[random.Next(specialChars.Length)];

        // Fill the rest randomly
        string allChars = upperChars + lowerChars + digitChars + specialChars;
        for (int i = 4; i < length; i++)
        {
            password[i] = allChars[random.Next(allChars.Length)];
        }

        // Shuffle the password
        for (int i = password.Length - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (password[i], password[j]) = (password[j], password[i]);
        }

        return new string(password);
    }
}
