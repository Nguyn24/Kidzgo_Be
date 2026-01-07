using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Authentication.Profiles.RequestParentPinReset;

public sealed class RequestParentPinResetCommand : ICommand
{
    public Guid ProfileId { get; init; }
}

