using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Media.PublishMedia;

public sealed record PublishMediaCommand(Guid Id) : ICommand<PublishMediaResponse>;

