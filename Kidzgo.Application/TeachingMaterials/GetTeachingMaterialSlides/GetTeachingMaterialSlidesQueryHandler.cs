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

        var slidesResult = await TeachingMaterialSlideHelper.EnsureSlidesAsync(
            context,
            storageService,
            previewService,
            material,
            cancellationToken);

        if (slidesResult.IsFailure)
        {
            return Result.Failure<GetTeachingMaterialSlidesResponse>(slidesResult.Error);
        }

        return MapResponse(material, slidesResult.Value);
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
}
