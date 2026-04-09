using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.TeachingMaterials.CreateTeachingMaterialAnnotation;
using Kidzgo.Application.TeachingMaterials.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.TeachingMaterials;
using Kidzgo.Domain.TeachingMaterials.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TeachingMaterials.UpdateTeachingMaterialAnnotation;

public sealed class UpdateTeachingMaterialAnnotationCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<UpdateTeachingMaterialAnnotationCommand, TeachingMaterialAnnotationResponse>
{
    public async Task<Result<TeachingMaterialAnnotationResponse>> Handle(
        UpdateTeachingMaterialAnnotationCommand command,
        CancellationToken cancellationToken)
    {
        var annotation = await context.TeachingMaterialAnnotations
            .Include(item => item.User)
            .FirstOrDefaultAsync(item => item.Id == command.AnnotationId, cancellationToken);

        if (annotation is null)
        {
            return Result.Failure<TeachingMaterialAnnotationResponse>(
                TeachingMaterialErrors.AnnotationNotFound(command.AnnotationId));
        }

        if (annotation.UserId != userContext.UserId)
        {
            return Result.Failure<TeachingMaterialAnnotationResponse>(
                TeachingMaterialErrors.AccessDenied(annotation.TeachingMaterialId));
        }

        if (!Enum.TryParse<TeachingMaterialAnnotationType>(command.Type, ignoreCase: true, out var type))
        {
            return Result.Failure<TeachingMaterialAnnotationResponse>(
                TeachingMaterialErrors.UnsupportedFileType(command.Type));
        }

        if (!Enum.TryParse<TeachingMaterialAnnotationVisibility>(command.Visibility, ignoreCase: true, out var visibility))
        {
            return Result.Failure<TeachingMaterialAnnotationResponse>(
                TeachingMaterialErrors.AnnotationVisibilityNotAllowed(command.Visibility));
        }

        var role = await TeachingMaterialAccessHelper.GetCurrentUserRoleAsync(context, userContext, cancellationToken);
        if (!CreateTeachingMaterialAnnotationCommandHandler.CanCreateVisibility(role, visibility))
        {
            return Result.Failure<TeachingMaterialAnnotationResponse>(
                TeachingMaterialErrors.AnnotationVisibilityNotAllowed(visibility.ToString()));
        }

        annotation.Content = command.Content.Trim();
        annotation.Color = string.IsNullOrWhiteSpace(command.Color) ? "#FFD700" : command.Color.Trim();
        annotation.PositionX = command.PositionX;
        annotation.PositionY = command.PositionY;
        annotation.Type = type;
        annotation.Visibility = visibility;
        annotation.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return CreateTeachingMaterialAnnotationCommandHandler.Map(
            annotation,
            annotation.User.Name ?? annotation.User.Username ?? annotation.User.Email);
    }
}
