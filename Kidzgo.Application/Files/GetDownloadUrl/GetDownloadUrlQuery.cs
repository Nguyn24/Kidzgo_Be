using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Files.GetDownloadUrl;

public sealed class GetDownloadUrlQuery : IQuery<GetDownloadUrlResponse>
{
    public required string Url { get; init; }
}

public sealed class GetDownloadUrlResponse
{
    public string Url { get; init; } = null!;
}

