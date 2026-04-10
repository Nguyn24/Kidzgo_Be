using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Storage;
using Kidzgo.Application.TeachingMaterials.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.TeachingMaterials;
using Kidzgo.Domain.TeachingMaterials.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TeachingMaterials.GetTeachingMaterialSlides;

public sealed class GetTeachingMaterialSlidesQueryHandler(
    IDbContext context,
    ITeachingMaterialStorageService storageService,
    ITeachingMaterialPreviewService previewService,
    IUserContext userContext
) : IQueryHandler<GetTeachingMaterialSlidesQuery, GetTeachingMaterialSlidesResponse>
{
    public async Task<Result<GetTeachingMaterialSlidesResponse>> Handle(
        GetTeachingMaterialSlidesQuery query,
        CancellationToken cancellationToken)
    {
        var material = await context.TeachingMaterials
            .FirstOrDefaultAsync(tm => tm.Id == query.TeachingMaterialId, cancellationToken);

        if (material is null)
        {
            return Result.Failure<GetTeachingMaterialSlidesResponse>(
                TeachingMaterialErrors.NotFound(query.TeachingMaterialId));
        }

        if (!await TeachingMaterialAccessHelper.CanReadAsync(context, userContext, material, cancellationToken))
        {
            return Result.Failure<GetTeachingMaterialSlidesResponse>(
                TeachingMaterialErrors.AccessDenied(query.TeachingMaterialId));
        }

        if (material.FileType != TeachingMaterialFileType.Presentation)
        {
            return Result.Failure<GetTeachingMaterialSlidesResponse>(TeachingMaterialErrors.NotAPresentation());
        }

        List<TeachingMaterialSlide> slides;
        try
        {
            slides = await EnsureSlidesAsync(material, cancellationToken);
        }
        catch (TeachingMaterialStoredFileMissingException)
        {
            return Result.Failure<GetTeachingMaterialSlidesResponse>(
                TeachingMaterialErrors.StoredFileMissing(query.TeachingMaterialId));
        }
        catch (TeachingMaterialSlideGenerationException)
        {
            return Result.Failure<GetTeachingMaterialSlidesResponse>(
                TeachingMaterialErrors.SlideGenerationFailed(query.TeachingMaterialId));
        }

        return MapResponse(material, slides);
    }

    private async Task<List<TeachingMaterialSlide>> EnsureSlidesAsync(
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
            throw new TeachingMaterialStoredFileMissingException();
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
            throw new TeachingMaterialSlideGenerationException();
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

    private static GetTeachingMaterialSlidesResponse MapResponse(
        TeachingMaterial material,
        IReadOnlyCollection<TeachingMaterialSlide> slides)
    {
        return new GetTeachingMaterialSlidesResponse
        {
            MaterialId = material.Id,
            DisplayName = material.DisplayName,
            TotalSlides = slides.Count,
            Slides = slides
                .OrderBy(slide => slide.SlideNumber)
                .Select(slide => new TeachingMaterialSlideDto
                {
                    SlideNumber = slide.SlideNumber,
                    Width = slide.Width,
                    Height = slide.Height,
                    PreviewUrl = $"/api/teaching-materials/{material.Id}/slides/{slide.SlideNumber}/preview",
                    ThumbnailUrl = $"/api/teaching-materials/{material.Id}/slides/{slide.SlideNumber}/thumbnail",
                    HasNotes = !string.IsNullOrWhiteSpace(slide.Notes)
                })
                .ToList()
        };
    }

    private sealed class TeachingMaterialSlideGenerationException : Exception;
    private sealed class TeachingMaterialStoredFileMissingException : Exception;
}
