using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Storage;
using Kidzgo.Application.TeachingMaterials.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.TeachingMaterials;
using Kidzgo.Domain.TeachingMaterials.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TeachingMaterials.GetTeachingMaterialSlideImage;

public sealed class GetTeachingMaterialSlideImageQueryHandler(
    IDbContext context,
    ITeachingMaterialStorageService storageService,
    IUserContext userContext
) : IQueryHandler<GetTeachingMaterialSlideImageQuery, GetTeachingMaterialSlideImageResponse>
{
    public async Task<Result<GetTeachingMaterialSlideImageResponse>> Handle(
        GetTeachingMaterialSlideImageQuery query,
        CancellationToken cancellationToken)
    {
        var material = await context.TeachingMaterials
            .FirstOrDefaultAsync(tm => tm.Id == query.TeachingMaterialId, cancellationToken);

        if (material is null)
        {
            return Result.Failure<GetTeachingMaterialSlideImageResponse>(
                TeachingMaterialErrors.NotFound(query.TeachingMaterialId));
        }

        if (!await TeachingMaterialAccessHelper.CanReadAsync(context, userContext, material, cancellationToken))
        {
            return Result.Failure<GetTeachingMaterialSlideImageResponse>(
                TeachingMaterialErrors.AccessDenied(query.TeachingMaterialId));
        }

        if (material.FileType != TeachingMaterialFileType.Presentation)
        {
            return Result.Failure<GetTeachingMaterialSlideImageResponse>(TeachingMaterialErrors.NotAPresentation());
        }

        var totalSlides = await context.TeachingMaterialSlides
            .CountAsync(slide => slide.TeachingMaterialId == material.Id, cancellationToken);

        var slide = await context.TeachingMaterialSlides
            .FirstOrDefaultAsync(
                item => item.TeachingMaterialId == material.Id && item.SlideNumber == query.SlideNumber,
                cancellationToken);

        if (slide is null)
        {
            return Result.Failure<GetTeachingMaterialSlideImageResponse>(
                TeachingMaterialErrors.SlideNotFound(query.SlideNumber, totalSlides));
        }

        var cachePath = query.ImageKind == TeachingMaterialSlideImageKind.Preview
            ? slide.PreviewImagePath
            : slide.ThumbnailImagePath;

        var fileName = query.ImageKind == TeachingMaterialSlideImageKind.Preview
            ? $"{material.Id:N}_slide_{query.SlideNumber}.png"
            : $"{material.Id:N}_thumb_{query.SlideNumber}.png";

        var cached = await storageService.ReadCacheFileAsync(
            cachePath,
            "image/png",
            fileName,
            cancellationToken);

        if (cached is null)
        {
            return Result.Failure<GetTeachingMaterialSlideImageResponse>(
                TeachingMaterialErrors.SlideGenerationFailed(material.Id));
        }

        return new GetTeachingMaterialSlideImageResponse
        {
            FileName = cached.FileName,
            MimeType = cached.MimeType,
            Content = cached.Content
        };
    }
}
