using System.Security.Cryptography;

namespace XdtBox.LicenseWeb.Services;

public sealed class LicenseWebPasswordHasher
{
    private const int Iterations = 100_000;
    private const int SaltBytes = 16;
    private const int HashBytes = 32;

    public PasswordHashResult HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var salt = RandomNumberGenerator.GetBytes(SaltBytes);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashBytes);
        return new PasswordHashResult(Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    public bool Verify(string password, string hashBase64, string saltBase64)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(hashBase64) || string.IsNullOrWhiteSpace(saltBase64))
        {
            return false;
        }

        byte[] expectedHash;
        byte[] salt;
        try
        {
            expectedHash = Convert.FromBase64String(hashBase64);
            salt = Convert.FromBase64String(saltBase64);
        }
        catch (FormatException)
        {
            return false;
        }

        var actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, expectedHash.Length);
        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}

public sealed record PasswordHashResult(string HashBase64, string SaltBase64);
