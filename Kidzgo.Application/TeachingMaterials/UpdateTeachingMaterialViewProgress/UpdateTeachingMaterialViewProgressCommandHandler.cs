using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.TeachingMaterials.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.TeachingMaterials;
using Kidzgo.Domain.TeachingMaterials.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TeachingMaterials.UpdateTeachingMaterialViewProgress;

public sealed class UpdateTeachingMaterialViewProgressCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<UpdateTeachingMaterialViewProgressCommand, TeachingMaterialViewProgressResponse>
{
    public async Task<Result<TeachingMaterialViewProgressResponse>> Handle(
        UpdateTeachingMaterialViewProgressCommand command,
        CancellationToken cancellationToken)
    {
        if (command.ProgressPercent is < 0 or > 100)
        {
            return Result.Failure<TeachingMaterialViewProgressResponse>(
                TeachingMaterialErrors.ProgressPercentInvalid());
        }

        var material = await context.TeachingMaterials
            .FirstOrDefaultAsync(tm => tm.Id == command.TeachingMaterialId, cancellationToken);

        if (material is null)
        {
            return Result.Failure<TeachingMaterialViewProgressResponse>(
                TeachingMaterialErrors.NotFound(command.TeachingMaterialId));
        }

        if (!await TeachingMaterialAccessHelper.CanReadAsync(context, userContext, material, cancellationToken))
        {
            return Result.Failure<TeachingMaterialViewProgressResponse>(
                TeachingMaterialErrors.AccessDenied(command.TeachingMaterialId));
        }

        var now = DateTime.UtcNow;
        var progress = await context.TeachingMaterialViewProgresses
            .FirstOrDefaultAsync(
                item => item.TeachingMaterialId == material.Id && item.UserId == userContext.UserId,
                cancellationToken);

        if (progress is null)
        {
            progress = new TeachingMaterialViewProgress
            {
                Id = Guid.NewGuid(),
                TeachingMaterialId = material.Id,
                UserId = userContext.UserId,
                FirstViewedAt = now,
                LastViewedAt = now,
                ViewCount = 1
            };

            progress.ApplyProgress(command.ProgressPercent, command.LastSlideViewed, command.TotalTimeSeconds, now);
            context.TeachingMaterialViewProgresses.Add(progress);
        }
        else
        {
            progress.ViewCount += 1;
            progress.ApplyProgress(command.ProgressPercent, command.LastSlideViewed, command.TotalTimeSeconds, now);
        }

        await context.SaveChangesAsync(cancellationToken);

        return Map(progress);
    }

    internal static TeachingMaterialViewProgressResponse Map(TeachingMaterialViewProgress progress) =>
        new()
        {
            MaterialId = progress.TeachingMaterialId,
            UserId = progress.UserId,
            ProgressPercent = progress.ProgressPercent,
            LastSlideViewed = progress.LastSlideViewed,
            TotalTimeSeconds = progress.TotalTimeSeconds,
            FirstViewedAt = progress.FirstViewedAt,
            LastViewedAt = progress.LastViewedAt,
            ViewCount = progress.ViewCount,
            Completed = progress.Completed
        };
}
