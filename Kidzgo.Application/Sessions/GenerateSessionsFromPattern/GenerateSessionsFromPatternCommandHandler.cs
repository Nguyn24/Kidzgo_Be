using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Sessions.GenerateSessionsFromPattern;

public sealed class GenerateSessionsFromPatternCommandHandler(
    IDbContext context,
    SessionGenerationService sessionGenerationService
) : ICommandHandler<GenerateSessionsFromPatternCommand, GenerateSessionsFromPatternResponse>
{
    public async Task<Result<GenerateSessionsFromPatternResponse>> Handle(
        GenerateSessionsFromPatternCommand command,
        CancellationToken cancellationToken)
    {
        var classEntity = await context.Classes
            .FirstOrDefaultAsync(c => c.Id == command.ClassId, cancellationToken);

        if (classEntity is null)
        {
            return Result.Failure<GenerateSessionsFromPatternResponse>(
                ClassErrors.NotFound(command.ClassId));
        }

        var generateResult = await sessionGenerationService.GenerateSessionsFromPatternAsync(
            classEntity,
            command.RoomId,
            command.OnlyFutureSessions,
            cancellationToken);

        if (generateResult.IsFailure)
        {
            return Result.Failure<GenerateSessionsFromPatternResponse>(generateResult.Error);
        }

        return Result.Success(new GenerateSessionsFromPatternResponse
        {
            CreatedSessionsCount = generateResult.Value
        });
    }
}


