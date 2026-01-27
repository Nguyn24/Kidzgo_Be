using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.CRM;
using Kidzgo.Domain.CRM.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.PlacementTests.MarkPlacementTestNoShow;

public sealed class MarkPlacementTestNoShowCommandHandler(
    IDbContext context
) : ICommandHandler<MarkPlacementTestNoShowCommand, MarkPlacementTestNoShowResponse>
{
    public async Task<Result<MarkPlacementTestNoShowResponse>> Handle(
        MarkPlacementTestNoShowCommand command,
        CancellationToken cancellationToken)
    {
        // UC-031: Mark Placement Test as NoShow
        var placementTest = await context.PlacementTests
            .FirstOrDefaultAsync(pt => pt.Id == command.PlacementTestId, cancellationToken);

        if (placementTest is null)
        {
            return Result.Failure<MarkPlacementTestNoShowResponse>(
                PlacementTestErrors.NotFound(command.PlacementTestId));
        }

        // Cannot mark completed test as NoShow
        if (placementTest.Status == PlacementTestStatus.Completed)
        {
            return Result.Failure<MarkPlacementTestNoShowResponse>(
                PlacementTestErrors.CannotMarkNoShowCompletedTest);
        }

        placementTest.Status = PlacementTestStatus.NoShow;
        placementTest.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return new MarkPlacementTestNoShowResponse
        {
            Id = placementTest.Id,
            Status = placementTest.Status.ToString(),
            UpdatedAt = placementTest.UpdatedAt
        };
    }
}

