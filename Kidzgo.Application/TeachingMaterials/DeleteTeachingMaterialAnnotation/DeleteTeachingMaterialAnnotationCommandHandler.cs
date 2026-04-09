using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.TeachingMaterials.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TeachingMaterials.DeleteTeachingMaterialAnnotation;

public sealed class DeleteTeachingMaterialAnnotationCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<DeleteTeachingMaterialAnnotationCommand>
{
    public async Task<Result> Handle(DeleteTeachingMaterialAnnotationCommand command, CancellationToken cancellationToken)
    {
        var annotation = await context.TeachingMaterialAnnotations
            .FirstOrDefaultAsync(item => item.Id == command.AnnotationId, cancellationToken);

        if (annotation is null)
        {
            return Result.Failure(TeachingMaterialErrors.AnnotationNotFound(command.AnnotationId));
        }

        if (annotation.UserId != userContext.UserId)
        {
            return Result.Failure(TeachingMaterialErrors.AccessDenied(annotation.TeachingMaterialId));
        }

        context.TeachingMaterialAnnotations.Remove(annotation);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
