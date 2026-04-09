using System.Security.Cryptography;
using System.Text;
using Kidzgo.Application.Abstraction.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Kidzgo.Infrastructure.Services;

public sealed class TeachingMaterialStorageService(
    IConfiguration configuration,
    ILogger<TeachingMaterialStorageService> logger
) : ITeachingMaterialStorageService
{
    private const int IvLength = 16;
    private readonly byte[] _key = DeriveKey(configuration);
    private readonly string _storageRoot = ResolveStorageRoot(configuration);

    public async Task<TeachingMaterialStorageResult> SaveEncryptedAsync(
        Stream input,
        string fileName,
        string storageFolder,
        CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_storageRoot);

        var normalizedFolder = NormalizeStorageFolder(storageFolder);
        var targetFolder = Path.Combine(_storageRoot, normalizedFolder);
        Directory.CreateDirectory(targetFolder);

        var encryptedFileName = $"{Guid.NewGuid():N}.bin";
        var absolutePath = Path.Combine(targetFolder, encryptedFileName);

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        await using var output = new FileStream(
            absolutePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            81920,
            useAsync: true);

        await output.WriteAsync(aes.IV, cancellationToken);
        await using (var cryptoStream = new CryptoStream(output, aes.CreateEncryptor(), CryptoStreamMode.Write, leaveOpen: true))
        {
            await input.CopyToAsync(cryptoStream, cancellationToken);
            cryptoStream.FlushFinalBlock();
        }

        logger.LogInformation("Encrypted teaching material stored at {Path} for file {FileName}", absolutePath, fileName);

        return new TeachingMaterialStorageResult
        {
            StoragePath = Path.Combine(normalizedFolder, encryptedFileName),
            EncryptionAlgorithm = "AES-256-CBC",
            EncryptionKeyVersion = "v1"
        };
    }

    public async Task<TeachingMaterialDownloadResult?> ReadDecryptedAsync(
        string storagePath,
        string originalFileName,
        string mimeType,
        CancellationToken cancellationToken = default)
    {
        var absolutePath = Path.Combine(_storageRoot, storagePath);
        if (!File.Exists(absolutePath))
        {
            return null;
        }

        await using var input = new FileStream(
            absolutePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            81920,
            useAsync: true);

        var iv = new byte[IvLength];
        var read = await input.ReadAsync(iv, cancellationToken);
        if (read != IvLength)
        {
            return null;
        }

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        await using var cryptoStream = new CryptoStream(input, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using var output = new MemoryStream();
        await cryptoStream.CopyToAsync(output, cancellationToken);

        return new TeachingMaterialDownloadResult
        {
            Content = output.ToArray(),
            FileName = originalFileName,
            MimeType = mimeType
        };
    }

    public async Task<TeachingMaterialCacheResult?> ReadCacheFileAsync(
        string cachePath,
        string mimeType,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var absolutePath = ResolveSafePath(_storageRoot, cachePath);
        if (!File.Exists(absolutePath))
        {
            return null;
        }

        var content = await File.ReadAllBytesAsync(absolutePath, cancellationToken);

        return new TeachingMaterialCacheResult
        {
            Content = content,
            FileName = fileName,
            MimeType = mimeType
        };
    }

    private static byte[] DeriveKey(IConfiguration configuration)
    {
        var secret = configuration["TeachingMaterials:EncryptionSecret"]
                     ?? configuration["Jwt:Secret"]
                     ?? throw new InvalidOperationException("Missing TeachingMaterials encryption secret");

        return SHA256.HashData(Encoding.UTF8.GetBytes(secret));
    }

    private static string ResolveStorageRoot(IConfiguration configuration)
    {
        var configuredPath = configuration["TeachingMaterials:StoragePath"];
        if (!string.IsNullOrWhiteSpace(configuredPath))
        {
            return configuredPath;
        }

        var publicStorageBasePath = configuration["FileStorage:Local:BasePath"];
        if (!string.IsNullOrWhiteSpace(publicStorageBasePath))
        {
            var parentPath = Directory.GetParent(publicStorageBasePath)?.FullName;
            if (!string.IsNullOrWhiteSpace(parentPath))
            {
                return Path.Combine(parentPath, "kidzgo-teaching-materials-private");
            }
        }

        return Path.Combine(AppContext.BaseDirectory, "kidzgo-teaching-materials-private");
    }

    private static string NormalizeStorageFolder(string storageFolder)
    {
        if (string.IsNullOrWhiteSpace(storageFolder))
        {
            return "general";
        }

        var normalized = storageFolder.Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar)
            .Trim(Path.DirectorySeparatorChar);

        return string.IsNullOrWhiteSpace(normalized) ? "general" : normalized;
    }

    private static string ResolveSafePath(string root, string relativePath)
    {
        var fullRoot = Path.GetFullPath(root);
        var fullPath = Path.GetFullPath(Path.Combine(fullRoot, relativePath));
        if (!fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Teaching material cache path is outside storage root");
        }

        return fullPath;
    }
}
