namespace Kidzgo.Application.TeachingMaterials.DownloadTeachingMaterial;

public sealed class DownloadTeachingMaterialResponse
{
    public string FileName { get; init; } = null!;
    public string MimeType { get; init; } = null!;
    public byte[] Content { get; init; } = Array.Empty<byte>();
}
