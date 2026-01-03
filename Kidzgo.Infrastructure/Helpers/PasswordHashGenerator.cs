using System.Security.Cryptography;

namespace Kidzgo.Infrastructure.Helpers;

/// <summary>
/// Helper to generate password hash with fixed salt for seed data
/// </summary>
public static class PasswordHashGenerator
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 500000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA512;

    /// <summary>
    /// Generate password hash with fixed salt for seed data consistency
    /// </summary>
    public static string GenerateHashWithFixedSalt(string password, string fixedSaltHex)
    {
        byte[] salt = Convert.FromHexString(fixedSaltHex);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);
        return $"{Convert.ToHexString(hash)}-{Convert.ToHexString(salt)}";
    }

    /// <summary>
    /// Generate password hash for seed data (Password: Password123!)
    /// </summary>
    public static string GenerateSeedPasswordHash()
    {
        const string password = "Password123!";
        const string fixedSalt = "0123456789ABCDEF0123456789ABCDEF";
        return GenerateHashWithFixedSalt(password, fixedSalt);
    }
}

