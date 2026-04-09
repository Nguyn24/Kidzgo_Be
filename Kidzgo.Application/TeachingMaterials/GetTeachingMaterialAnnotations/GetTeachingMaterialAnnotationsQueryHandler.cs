using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.TeachingMaterials.CreateTeachingMaterialAnnotation;
using Kidzgo.Application.TeachingMaterials.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.TeachingMaterials;
using Kidzgo.Domain.TeachingMaterials.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TeachingMaterials.GetTeachingMaterialAnnotations;

public sealed class GetTeachingMaterialAnnotationsQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetTeachingMaterialAnnotationsQuery, IReadOnlyCollection<TeachingMaterialAnnotationResponse>>
{
    public async Task<Result<IReadOnlyCollection<TeachingMaterialAnnotationResponse>>> Handle(
        GetTeachingMaterialAnnotationsQuery query,
        CancellationToken cancellationToken)
    {
        var material = await context.TeachingMaterials
            .FirstOrDefaultAsync(tm => tm.Id == query.TeachingMaterialId, cancellationToken);

        if (material is null)
        {
            return Result.Failure<IReadOnlyCollection<TeachingMaterialAnnotationResponse>>(
                TeachingMaterialErrors.NotFound(query.TeachingMaterialId));
        }

        if (!await TeachingMaterialAccessHelper.CanReadAsync(context, userContext, material, cancellationToken))
        {
            return Result.Failure<IReadOnlyCollection<TeachingMaterialAnnotationResponse>>(
                TeachingMaterialErrors.AccessDenied(query.TeachingMaterialId));
        }

        var annotationsQuery = context.TeachingMaterialAnnotations
            .AsNoTracking()
            .Include(annotation => annotation.User)
            .Where(annotation => annotation.TeachingMaterialId == material.Id);

        if (query.SlideNumber.HasValue)
        {
            annotationsQuery = annotationsQuery.Where(annotation => annotation.SlideNumber == query.SlideNumber);
        }

        if (!query.Visibility.Equals("All", StringComparison.OrdinalIgnoreCase) &&
            Enum.TryParse<TeachingMaterialAnnotationVisibility>(query.Visibility, ignoreCase: true, out var visibility))
        {
            annotationsQuery = annotationsQuery.Where(annotation => annotation.Visibility == visibility);
        }

        annotationsQuery = annotationsQuery.Where(annotation =>
            annotation.UserId == userContext.UserId ||
            annotation.Visibility == TeachingMaterialAnnotationVisibility.Class ||
            annotation.Visibility == TeachingMaterialAnnotationVisibility.Public);

        var annotations = await annotationsQuery
            .OrderBy(annotation => annotation.SlideNumber)
            .ThenBy(annotation => annotation.CreatedAt)
            .Select(annotation => new TeachingMaterialAnnotationResponse
            {
                Id = annotation.Id,
                SlideNumber = annotation.SlideNumber,
                Content = annotation.Content,
                Color = annotation.Color,
                PositionX = annotation.PositionX,
                PositionY = annotation.PositionY,
                Type = annotation.Type.ToString(),
                Visibility = annotation.Visibility.ToString(),
                CreatedByUserId = annotation.UserId,
                CreatedByName = annotation.User.Name ?? annotation.User.Username ?? annotation.User.Email,
                CreatedAt = annotation.CreatedAt,
                UpdatedAt = annotation.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return annotations;
    }
}
