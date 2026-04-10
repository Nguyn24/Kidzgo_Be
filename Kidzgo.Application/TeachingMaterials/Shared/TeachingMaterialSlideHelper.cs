using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Storage;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.TeachingMaterials;
using Kidzgo.Domain.TeachingMaterials.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TeachingMaterials.Shared;

internal static class TeachingMaterialSlideHelper
{
    public static async Task<Result<List<TeachingMaterialSlide>>> EnsureSlidesAsync(
        IDbContext context,
        ITeachingMaterialStorageService storageService,
        ITeachingMaterialPreviewService previewService,
        TeachingMaterial material,
        CancellationToken cancellationToken)
    {
        var slides = await context.TeachingMaterialSlides
            .Where(slide => slide.TeachingMaterialId == material.Id)
            .OrderBy(slide => slide.SlideNumber)
            .ToListAsync(cancellationToken);

        if (slides.Count > 0)
        {
            return slides;
        }

        var file = await storageService.ReadDecryptedAsync(
            material.StoragePath,
            material.OriginalFileName,
            material.MimeType,
            cancellationToken);

        if (file is null)
        {
            return Result.Failure<List<TeachingMaterialSlide>>(
                TeachingMaterialErrors.StoredFileMissing(material.Id));
        }

        IReadOnlyList<TeachingMaterialSlidePreviewFile> generated;
        try
        {
            generated = await previewService.GenerateSlidePreviewsAsync(
                material.Id,
                material.OriginalFileName,
                file.Content,
                cancellationToken);
        }
        catch
        {
            return Result.Failure<List<TeachingMaterialSlide>>(
                TeachingMaterialErrors.SlideGenerationFailed(material.Id));
        }

        var generatedAt = DateTime.UtcNow;
        slides = generated
            .Select(item => new TeachingMaterialSlide
            {
                Id = Guid.NewGuid(),
                TeachingMaterialId = material.Id,
                SlideNumber = item.SlideNumber,
                PreviewImagePath = item.PreviewImagePath,
                ThumbnailImagePath = item.ThumbnailImagePath,
                Width = item.Width,
                Height = item.Height,
                Notes = item.Notes,
                GeneratedAt = generatedAt
            })
            .ToList();

        context.TeachingMaterialSlides.AddRange(slides);
        await context.SaveChangesAsync(cancellationToken);

        return slides;
    }
}
