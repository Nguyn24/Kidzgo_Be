using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.CRM;
using Kidzgo.Domain.CRM.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.PlacementTests.CancelPlacementTest;

public sealed class CancelPlacementTestCommandHandler(
    IDbContext context
) : ICommandHandler<CancelPlacementTestCommand, CancelPlacementTestResponse>
{
    public async Task<Result<CancelPlacementTestResponse>> Handle(
        CancelPlacementTestCommand command,
        CancellationToken cancellationToken)
    {
        // UC-030: Cancel Placement Test
        var placementTest = await context.PlacementTests
            .FirstOrDefaultAsync(pt => pt.Id == command.PlacementTestId, cancellationToken);

        if (placementTest is null)
        {
            return Result.Failure<CancelPlacementTestResponse>(
                PlacementTestErrors.NotFound(command.PlacementTestId));
        }

        // Cannot cancel completed test
        if (placementTest.Status == PlacementTestStatus.Completed)
        {
            return Result.Failure<CancelPlacementTestResponse>(
                PlacementTestErrors.CannotCancelCompletedTest);
        }

        placementTest.Status = PlacementTestStatus.Cancelled;
        if (!string.IsNullOrWhiteSpace(command.Reason))
        {
            placementTest.Notes = string.IsNullOrWhiteSpace(placementTest.Notes)
                ? $"Cancelled: {command.Reason.Trim()}"
                : $"{placementTest.Notes}\nCancelled: {command.Reason.Trim()}";
        }

        placementTest.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return new CancelPlacementTestResponse
        {
            Id = placementTest.Id,
            Status = placementTest.Status.ToString(),
            UpdatedAt = placementTest.UpdatedAt
        };
    }
}

