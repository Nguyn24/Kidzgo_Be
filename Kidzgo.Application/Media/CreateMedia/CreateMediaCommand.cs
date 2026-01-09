using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Media;

namespace Kidzgo.Application.Media.CreateMedia;

public sealed record CreateMediaCommand(
    Guid BranchId,
    Guid? ClassId,
    Guid? StudentProfileId,
    string? MonthTag,
    MediaType Type,
    MediaContentType ContentType,
    string Url,
    string? Caption,
    Visibility Visibility
) : ICommand<CreateMediaResponse>;

