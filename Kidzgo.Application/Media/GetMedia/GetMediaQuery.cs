using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Media;

namespace Kidzgo.Application.Media.GetMedia;

public sealed record GetMediaQuery(
    Guid? BranchId,
    Guid? ClassId,
    string? MonthTag,
    MediaType? Type,
    MediaContentType? ContentType,
    Visibility? Visibility,
    ApprovalStatus? ApprovalStatus,
    bool? IsPublished,
    int PageNumber = 1,
    int PageSize = 20
) : IQuery<GetMediaResponse>;

