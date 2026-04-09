namespace Kidzgo.Application.TeachingMaterials.GetTeachingMaterialPreviewPdf;

public sealed class GetTeachingMaterialPreviewPdfResponse
{
    public string FileName { get; init; } = null!;
    public string MimeType { get; init; } = null!;
    public byte[] Content { get; init; } = Array.Empty<byte>();
}
