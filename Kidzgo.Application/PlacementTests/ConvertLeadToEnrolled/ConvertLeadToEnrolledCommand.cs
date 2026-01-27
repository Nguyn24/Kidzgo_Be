using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.PlacementTests.ConvertLeadToEnrolled;

public sealed class ConvertLeadToEnrolledCommand : ICommand<ConvertLeadToEnrolledResponse>
{
    public Guid PlacementTestId { get; init; }
    public Guid? StudentProfileId { get; init; }
}

