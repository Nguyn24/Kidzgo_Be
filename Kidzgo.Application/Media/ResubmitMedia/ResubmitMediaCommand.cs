using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Media.ResubmitMedia;

public sealed record ResubmitMediaCommand(Guid Id) : ICommand<ResubmitMediaResponse>;
