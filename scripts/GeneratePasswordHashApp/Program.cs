using System;
using System.Security.Cryptography;

// Helper to generate password hash for seed data
// Run: dotnet run --project scripts/GeneratePasswordHashApp/GeneratePasswordHashApp.csproj

const string password = "Password123!";
const int HashSize = 32;
const int Iterations = 500000;
var Algorithm = HashAlgorithmName.SHA512;

// Use fixed salt for seed data (so hash is consistent across migrations)
byte[] salt = Convert.FromHexString("0123456789ABCDEF0123456789ABCDEF");
byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);

string passwordHash = $"{Convert.ToHexString(hash)}-{Convert.ToHexString(salt)}";

Console.WriteLine("=== Password Hash Generator ===");
Console.WriteLine($"Password: {password}");
Console.WriteLine($"Password Hash: {passwordHash}");
Console.WriteLine("\nCopy the hash above to seed SQL script");

