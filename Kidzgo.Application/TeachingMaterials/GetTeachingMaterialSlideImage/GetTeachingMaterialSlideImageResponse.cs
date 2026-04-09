namespace Kidzgo.Application.TeachingMaterials.GetTeachingMaterialSlideImage;

public sealed class GetTeachingMaterialSlideImageResponse
{
    public string FileName { get; init; } = null!;
    public string MimeType { get; init; } = null!;
    public byte[] Content { get; init; } = Array.Empty<byte>();
}
