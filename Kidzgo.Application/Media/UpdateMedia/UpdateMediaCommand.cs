using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Media;

namespace Kidzgo.Application.Media.UpdateMedia;

public sealed record UpdateMediaCommand(
    Guid Id,
    Guid? ClassId,
    Guid? StudentProfileId,
    string? MonthTag,
    MediaContentType? ContentType,
    string? Caption,
    Visibility? Visibility
) : ICommand<UpdateMediaResponse>;

