using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Gamification;
using Kidzgo.Domain.Gamification.Errors;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Gamification.DeductXp;

public sealed class DeductXpCommandHandler(
    IDbContext context,
    ILevelCalculationService levelCalculationService
) : ICommandHandler<DeductXpCommand, DeductXpResponse>
{
    public async Task<Result<DeductXpResponse>> Handle(
        DeductXpCommand command,
        CancellationToken cancellationToken)
    {
        // Validate student profile exists
        var profile = await context.Profiles
            .FirstOrDefaultAsync(p => p.Id == command.StudentProfileId && p.ProfileType == Domain.Users.ProfileType.Student, cancellationToken);

        if (profile == null)
        {
            return Result.Failure<DeductXpResponse>(
                Domain.Gamification.Errors.XpErrors.ProfileNotFound(command.StudentProfileId));
        }

        // Get StudentLevel
        var studentLevel = await context.StudentLevels
            .FirstOrDefaultAsync(sl => sl.StudentProfileId == command.StudentProfileId, cancellationToken);

        var oldLevel = studentLevel?.CurrentLevel ?? "Level 1";
        var oldXp = studentLevel?.CurrentXp ?? 0;
        var newXp = Math.Max(0, oldXp - command.Amount); // XP không thể âm
        var newLevel = levelCalculationService.CalculateLevel(newXp);
        var levelDown = oldLevel != newLevel;

        if (studentLevel == null)
        {
            studentLevel = new StudentLevel
            {
                Id = Guid.NewGuid(),
                StudentProfileId = command.StudentProfileId,
                CurrentLevel = newLevel,
                CurrentXp = newXp,
                UpdatedAt = DateTime.UtcNow
            };
            context.StudentLevels.Add(studentLevel);
        }
        else
        {
            studentLevel.CurrentLevel = newLevel;
            studentLevel.CurrentXp = newXp;
            studentLevel.UpdatedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new DeductXpResponse
        {
            StudentProfileId = command.StudentProfileId,
            Amount = command.Amount,
            NewXp = newXp,
            NewLevel = newLevel,
            LevelDown = levelDown
        });
    }
}

