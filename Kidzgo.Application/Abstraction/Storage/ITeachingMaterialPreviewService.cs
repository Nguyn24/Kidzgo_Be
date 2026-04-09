namespace Kidzgo.Application.Abstraction.Storage;

public interface ITeachingMaterialPreviewService
{
    Task<TeachingMaterialPdfPreviewFile> GeneratePdfPreviewAsync(
        Guid materialId,
        string originalFileName,
        byte[] content,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TeachingMaterialSlidePreviewFile>> GenerateSlidePreviewsAsync(
        Guid materialId,
        string originalFileName,
        byte[] content,
        CancellationToken cancellationToken = default);
}

public sealed class TeachingMaterialPdfPreviewFile
{
    public string CachePath { get; init; } = null!;
    public byte[] Content { get; init; } = Array.Empty<byte>();
    public long FileSize => Content.LongLength;
}

public sealed class TeachingMaterialSlidePreviewFile
{
    public int SlideNumber { get; init; }
    public string PreviewImagePath { get; init; } = null!;
    public string ThumbnailImagePath { get; init; } = null!;
    public int Width { get; init; } = 1920;
    public int Height { get; init; } = 1080;
    public string? Notes { get; init; }
}
