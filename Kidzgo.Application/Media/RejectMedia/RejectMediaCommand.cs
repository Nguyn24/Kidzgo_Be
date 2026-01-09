using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Media.RejectMedia;

public sealed record RejectMediaCommand(Guid Id) : ICommand<RejectMediaResponse>;

