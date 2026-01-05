using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Authentication.Profiles.SelectStudentProfile;

public sealed class SelectStudentProfileCommand : ICommand
{
    public Guid ProfileId { get; init; }
}


