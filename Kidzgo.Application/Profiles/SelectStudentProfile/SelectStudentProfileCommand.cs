using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Profiles.SelectStudentProfile;

public sealed class SelectStudentProfileCommand : ICommand<SelectStudentProfileResponse>
{
    public Guid ProfileId { get; init; }
}

public sealed class SelectStudentProfileResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public Guid StudentId { get; init; }
}































