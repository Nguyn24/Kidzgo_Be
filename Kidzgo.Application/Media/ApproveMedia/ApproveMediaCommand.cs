using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Media.ApproveMedia;

public sealed record ApproveMediaCommand(Guid Id) : ICommand<ApproveMediaResponse>;

