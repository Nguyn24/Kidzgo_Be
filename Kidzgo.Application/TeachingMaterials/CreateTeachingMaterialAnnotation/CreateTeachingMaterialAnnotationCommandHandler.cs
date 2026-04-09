using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.TeachingMaterials.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.TeachingMaterials;
using Kidzgo.Domain.TeachingMaterials.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TeachingMaterials.CreateTeachingMaterialAnnotation;

public sealed class CreateTeachingMaterialAnnotationCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<CreateTeachingMaterialAnnotationCommand, TeachingMaterialAnnotationResponse>
{
    public async Task<Result<TeachingMaterialAnnotationResponse>> Handle(
        CreateTeachingMaterialAnnotationCommand command,
        CancellationToken cancellationToken)
    {
        var material = await context.TeachingMaterials
            .FirstOrDefaultAsync(tm => tm.Id == command.TeachingMaterialId, cancellationToken);

        if (material is null)
        {
            return Result.Failure<TeachingMaterialAnnotationResponse>(
                TeachingMaterialErrors.NotFound(command.TeachingMaterialId));
        }

        if (!await TeachingMaterialAccessHelper.CanReadAsync(context, userContext, material, cancellationToken))
        {
            return Result.Failure<TeachingMaterialAnnotationResponse>(
                TeachingMaterialErrors.AccessDenied(command.TeachingMaterialId));
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
        if (!CanCreateVisibility(role, visibility))
        {
            return Result.Failure<TeachingMaterialAnnotationResponse>(
                TeachingMaterialErrors.AnnotationVisibilityNotAllowed(visibility.ToString()));
        }

        var now = DateTime.UtcNow;
        var annotation = new TeachingMaterialAnnotation
        {
            Id = Guid.NewGuid(),
            TeachingMaterialId = material.Id,
            SlideNumber = command.SlideNumber,
            UserId = userContext.UserId,
            Content = command.Content.Trim(),
            Color = string.IsNullOrWhiteSpace(command.Color) ? "#FFD700" : command.Color.Trim(),
            PositionX = command.PositionX,
            PositionY = command.PositionY,
            Type = type,
            Visibility = visibility,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.TeachingMaterialAnnotations.Add(annotation);
        await context.SaveChangesAsync(cancellationToken);

        var user = await context.Users.AsNoTracking().FirstAsync(u => u.Id == userContext.UserId, cancellationToken);
        return Map(annotation, user.Name ?? user.Username ?? user.Email);
    }

    internal static TeachingMaterialAnnotationResponse Map(TeachingMaterialAnnotation annotation, string? userName) =>
        new()
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
            CreatedByName = userName,
            CreatedAt = annotation.CreatedAt,
            UpdatedAt = annotation.UpdatedAt
        };

    internal static bool CanCreateVisibility(UserRole? role, TeachingMaterialAnnotationVisibility visibility) =>
        visibility switch
        {
            TeachingMaterialAnnotationVisibility.Private => true,
            TeachingMaterialAnnotationVisibility.Class => role is UserRole.Admin or UserRole.Teacher,
            TeachingMaterialAnnotationVisibility.Public => role is UserRole.Admin,
            _ => false
        };
}
