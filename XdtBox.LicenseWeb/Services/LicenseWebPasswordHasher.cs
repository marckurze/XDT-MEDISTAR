using System.Security.Cryptography;

namespace XdtBox.LicenseWeb.Services;

public sealed class LicenseWebPasswordHasher
{
    public const int DefaultIterations = 100_000;
    private const int SaltBytes = 16;
    private const int HashBytes = 32;

    public PasswordHashResult HashPassword(string password, int iterations = DefaultIterations)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var effectiveIterations = NormalizeIterations(iterations);
        var salt = RandomNumberGenerator.GetBytes(SaltBytes);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, effectiveIterations, HashAlgorithmName.SHA256, HashBytes);
        return new PasswordHashResult(Convert.ToBase64String(hash), Convert.ToBase64String(salt), effectiveIterations);
    }

    public bool Verify(string password, string hashBase64, string saltBase64, int iterations = DefaultIterations)
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

        var actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, NormalizeIterations(iterations), HashAlgorithmName.SHA256, expectedHash.Length);
        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }

    private static int NormalizeIterations(int iterations)
    {
        return iterations >= 50_000 ? iterations : DefaultIterations;
    }
}

public sealed record PasswordHashResult(string HashBase64, string SaltBase64, int Iterations);
