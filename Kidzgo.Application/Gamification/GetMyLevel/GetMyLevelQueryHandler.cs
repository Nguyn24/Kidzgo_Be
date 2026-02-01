using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Gamification.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Gamification.GetMyLevel;

public sealed class GetMyLevelQueryHandler(
    IDbContext context,
    IUserContext userContext,
    ILevelCalculationService levelCalculationService
) : IQueryHandler<GetMyLevelQuery, GetMyLevelResponse>
{
    public async Task<Result<GetMyLevelResponse>> Handle(
        GetMyLevelQuery query,
        CancellationToken cancellationToken)
    {
        if (!userContext.StudentId.HasValue)
        {
            return Result.Failure<GetMyLevelResponse>(
                XpErrors.ProfileNotFound(Guid.Empty));
        }

        var studentProfileId = userContext.StudentId.Value;

        var studentLevel = await context.StudentLevels
            .FirstOrDefaultAsync(sl => sl.StudentProfileId == studentProfileId, cancellationToken);

        var xp = studentLevel?.CurrentXp ?? 0;
        var level = studentLevel?.CurrentLevel ?? levelCalculationService.CalculateLevel(xp);
        var xpRequired = levelCalculationService.GetXpRequiredForNextLevel(level, xp);

        return Result.Success(new GetMyLevelResponse
        {
            StudentProfileId = studentProfileId,
            Level = level,
            Xp = xp,
            XpRequiredForNextLevel = xpRequired
        });
    }
}

