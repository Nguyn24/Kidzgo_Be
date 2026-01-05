using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Classes.CheckClassCapacity;

public sealed class CheckClassCapacityQuery : IQuery<CheckClassCapacityResponse>
{
    public Guid ClassId { get; init; }
}

