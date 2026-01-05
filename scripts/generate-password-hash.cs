using System.Security.Cryptography;
using System.Text;

// Helper script to generate password hash for seed data
// Usage: dotnet script generate-password-hash.cs

const string password = "Password123!";
const int SaltSize = 16;
const int HashSize = 32;
const int Iterations = 500000;
var Algorithm = HashAlgorithmName.SHA512;

// Use fixed salt for seed data (so hash is consistent)
byte[] salt = Convert.FromHexString("0123456789ABCDEF0123456789ABCDEF");
byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);

string passwordHash = $"{Convert.ToHexString(hash)}-{Convert.ToHexString(salt)}";

Console.WriteLine($"Password: {password}");
Console.WriteLine($"Password Hash: {passwordHash}");

