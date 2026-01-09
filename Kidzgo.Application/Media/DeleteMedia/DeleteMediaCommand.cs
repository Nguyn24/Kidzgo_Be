using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Media.DeleteMedia;

public sealed record DeleteMediaCommand(Guid Id) : ICommand;

