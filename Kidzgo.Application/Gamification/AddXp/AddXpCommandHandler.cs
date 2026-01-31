using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Gamification;
using Kidzgo.Domain.Gamification.Errors;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Gamification.AddXp;

public sealed class AddXpCommandHandler(
    IDbContext context,
    ILevelCalculationService levelCalculationService
) : ICommandHandler<AddXpCommand, AddXpResponse>
{
    public async Task<Result<AddXpResponse>> Handle(
        AddXpCommand command,
        CancellationToken cancellationToken)
    {
        // Validate student profile exists
        var profile = await context.Profiles
            .FirstOrDefaultAsync(p => p.Id == command.StudentProfileId && p.ProfileType == Domain.Users.ProfileType.Student, cancellationToken);

        if (profile == null)
        {
            return Result.Failure<AddXpResponse>(
                Domain.Gamification.Errors.XpErrors.ProfileNotFound(command.StudentProfileId));
        }

        // Get or create StudentLevel
        var studentLevel = await context.StudentLevels
            .FirstOrDefaultAsync(sl => sl.StudentProfileId == command.StudentProfileId, cancellationToken);

        var oldLevel = studentLevel?.CurrentLevel ?? "Level 1";
        var oldXp = studentLevel?.CurrentXp ?? 0;
        var newXp = oldXp + command.Amount;
        var newLevel = levelCalculationService.CalculateLevel(newXp);
        var levelUp = oldLevel != newLevel;

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

        return Result.Success(new AddXpResponse
        {
            StudentProfileId = command.StudentProfileId,
            Amount = command.Amount,
            NewXp = newXp,
            NewLevel = newLevel,
            LevelUp = levelUp
        });
    }
}

