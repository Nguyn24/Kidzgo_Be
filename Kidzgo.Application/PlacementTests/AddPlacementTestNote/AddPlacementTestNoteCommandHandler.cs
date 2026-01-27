using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.CRM;
using Kidzgo.Domain.CRM.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.PlacementTests.AddPlacementTestNote;

public sealed class AddPlacementTestNoteCommandHandler(
    IDbContext context
) : ICommandHandler<AddPlacementTestNoteCommand, AddPlacementTestNoteResponse>
{
    public async Task<Result<AddPlacementTestNoteResponse>> Handle(
        AddPlacementTestNoteCommand command,
        CancellationToken cancellationToken)
    {
        // UC-037: Add Note to Placement Test
        var placementTest = await context.PlacementTests
            .FirstOrDefaultAsync(pt => pt.Id == command.PlacementTestId, cancellationToken);

        if (placementTest is null)
        {
            return Result.Failure<AddPlacementTestNoteResponse>(
                PlacementTestErrors.NotFound(command.PlacementTestId));
        }

        var noteText = command.Note.Trim();
        placementTest.Notes = string.IsNullOrWhiteSpace(placementTest.Notes)
            ? noteText
            : $"{placementTest.Notes}\n{noteText}";

        placementTest.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return new AddPlacementTestNoteResponse
        {
            Id = placementTest.Id,
            Notes = placementTest.Notes,
            UpdatedAt = placementTest.UpdatedAt
        };
    }
}

