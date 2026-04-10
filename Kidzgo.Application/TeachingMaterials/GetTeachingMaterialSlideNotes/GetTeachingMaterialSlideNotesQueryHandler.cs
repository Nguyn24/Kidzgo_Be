using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Storage;
using Kidzgo.Application.TeachingMaterials.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.TeachingMaterials;
using Kidzgo.Domain.TeachingMaterials.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TeachingMaterials.GetTeachingMaterialSlideNotes;

public sealed class GetTeachingMaterialSlideNotesQueryHandler(
    IDbContext context,
    ITeachingMaterialStorageService storageService,
    ITeachingMaterialPreviewService previewService,
    IUserContext userContext
) : IQueryHandler<GetTeachingMaterialSlideNotesQuery, GetTeachingMaterialSlideNotesResponse>
{
    public async Task<Result<GetTeachingMaterialSlideNotesResponse>> Handle(
        GetTeachingMaterialSlideNotesQuery query,
        CancellationToken cancellationToken)
    {
        var material = await context.TeachingMaterials
            .FirstOrDefaultAsync(tm => tm.Id == query.TeachingMaterialId, cancellationToken);

        if (material is null)
        {
            return Result.Failure<GetTeachingMaterialSlideNotesResponse>(
                TeachingMaterialErrors.NotFound(query.TeachingMaterialId));
        }

        if (!await TeachingMaterialAccessHelper.CanReadAsync(context, userContext, material, cancellationToken))
        {
            return Result.Failure<GetTeachingMaterialSlideNotesResponse>(
                TeachingMaterialErrors.AccessDenied(query.TeachingMaterialId));
        }

        if (material.FileType != TeachingMaterialFileType.Presentation)
        {
            return Result.Failure<GetTeachingMaterialSlideNotesResponse>(TeachingMaterialErrors.NotAPresentation());
        }

        var slidesResult = await TeachingMaterialSlideHelper.EnsureSlidesAsync(
            context,
            storageService,
            previewService,
            material,
            cancellationToken);

        if (slidesResult.IsFailure)
        {
            return Result.Failure<GetTeachingMaterialSlideNotesResponse>(slidesResult.Error);
        }

        var slides = slidesResult.Value;
        var slide = slides.FirstOrDefault(item => item.SlideNumber == query.SlideNumber);

        if (slide is null)
        {
            return Result.Failure<GetTeachingMaterialSlideNotesResponse>(
                TeachingMaterialErrors.SlideNotFound(query.SlideNumber, slides.Count));
        }

        return new GetTeachingMaterialSlideNotesResponse
        {
            MaterialId = material.Id,
            SlideNumber = slide.SlideNumber,
            HasNotes = !string.IsNullOrWhiteSpace(slide.Notes),
            Notes = string.IsNullOrWhiteSpace(slide.Notes) ? null : slide.Notes
        };
    }
}
