namespace Kidzgo.Application.Abstraction.Storage;

public interface ITeachingMaterialStorageService
{
    Task<TeachingMaterialStorageResult> SaveEncryptedAsync(
        Stream input,
        string fileName,
        string storageFolder,
        CancellationToken cancellationToken = default);

    Task<TeachingMaterialDownloadResult?> ReadDecryptedAsync(
        string storagePath,
        string originalFileName,
        string mimeType,
        CancellationToken cancellationToken = default);

    Task<TeachingMaterialCacheResult?> ReadCacheFileAsync(
        string cachePath,
        string mimeType,
        string fileName,
        CancellationToken cancellationToken = default);
}

public sealed class TeachingMaterialStorageResult
{
    public string StoragePath { get; init; } = null!;
    public string EncryptionAlgorithm { get; init; } = null!;
    public string EncryptionKeyVersion { get; init; } = null!;
}

public sealed class TeachingMaterialDownloadResult
{
    public byte[] Content { get; init; } = Array.Empty<byte>();
    public string FileName { get; init; } = null!;
    public string MimeType { get; init; } = null!;
}

public sealed class TeachingMaterialCacheResult
{
    public byte[] Content { get; init; } = Array.Empty<byte>();
    public string FileName { get; init; } = null!;
    public string MimeType { get; init; } = null!;
}
