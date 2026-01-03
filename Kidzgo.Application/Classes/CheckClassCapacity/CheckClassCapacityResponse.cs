namespace Kidzgo.Application.Classes.CheckClassCapacity;

public sealed class CheckClassCapacityResponse
{
    public Guid ClassId { get; init; }
    public int Capacity { get; init; }
    public int CurrentEnrollmentCount { get; init; }
    public int AvailableSlots { get; init; }
    public bool HasAvailableSlots { get; init; }
}

