using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.TeachingMaterials.Shared;
using Kidzgo.Application.TeachingMaterials.UpdateTeachingMaterialViewProgress;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.TeachingMaterials.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TeachingMaterials.GetTeachingMaterialViewProgress;

public sealed class GetTeachingMaterialViewProgressQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetTeachingMaterialViewProgressQuery, TeachingMaterialViewProgressResponse>
{
    public async Task<Result<TeachingMaterialViewProgressResponse>> Handle(
        GetTeachingMaterialViewProgressQuery query,
        CancellationToken cancellationToken)
    {
        var material = await context.TeachingMaterials
            .FirstOrDefaultAsync(tm => tm.Id == query.TeachingMaterialId, cancellationToken);

        if (material is null)
        {
            return Result.Failure<TeachingMaterialViewProgressResponse>(
                TeachingMaterialErrors.NotFound(query.TeachingMaterialId));
        }

        if (!await TeachingMaterialAccessHelper.CanReadAsync(context, userContext, material, cancellationToken))
        {
            return Result.Failure<TeachingMaterialViewProgressResponse>(
                TeachingMaterialErrors.AccessDenied(query.TeachingMaterialId));
        }

        var progress = await context.TeachingMaterialViewProgresses
            .AsNoTracking()
            .FirstOrDefaultAsync(
                item => item.TeachingMaterialId == material.Id && item.UserId == userContext.UserId,
                cancellationToken);

        if (progress is null)
        {
            return Result.Failure<TeachingMaterialViewProgressResponse>(
                TeachingMaterialErrors.NotFound(query.TeachingMaterialId));
        }

        return UpdateTeachingMaterialViewProgressCommandHandler.Map(progress);
    }
}
