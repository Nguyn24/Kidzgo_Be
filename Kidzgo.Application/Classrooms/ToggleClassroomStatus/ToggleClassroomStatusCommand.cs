using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Classrooms.ToggleClassroomStatus;

public sealed class ToggleClassroomStatusCommand : ICommand<ToggleClassroomStatusResponse>
{
    public Guid Id { get; init; }
}

