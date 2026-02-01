using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Gamification.GetStudentLevel;

public sealed class GetStudentLevelQueryHandler(
    IDbContext context,
    ILevelCalculationService levelCalculationService
) : IQueryHandler<GetStudentLevelQuery, GetStudentLevelResponse>
{
    public async Task<Result<GetStudentLevelResponse>> Handle(
        GetStudentLevelQuery query,
        CancellationToken cancellationToken)
    {
        var studentLevel = await context.StudentLevels
            .FirstOrDefaultAsync(sl => sl.StudentProfileId == query.StudentProfileId, cancellationToken);

        var xp = studentLevel?.CurrentXp ?? 0;
        var level = studentLevel?.CurrentLevel ?? levelCalculationService.CalculateLevel(xp);
        var xpRequired = levelCalculationService.GetXpRequiredForNextLevel(level, xp);

        return Result.Success(new GetStudentLevelResponse
        {
            StudentProfileId = query.StudentProfileId,
            Level = level,
            Xp = xp,
            XpRequiredForNextLevel = xpRequired
        });
    }
}

