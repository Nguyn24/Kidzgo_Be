using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Media.GetMedia;

public sealed record GetMediaResponse
{
    public Page<MediaDto> Media { get; init; } = null!;
}

