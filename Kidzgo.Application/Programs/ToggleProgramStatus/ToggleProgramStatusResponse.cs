namespace Kidzgo.Application.Programs.ToggleProgramStatus;

public sealed class ToggleProgramStatusResponse
{
    public Guid Id { get; init; }
    public bool IsActive { get; init; }
}

