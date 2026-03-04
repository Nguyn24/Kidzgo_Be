using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Files.GetTransformedUrl;

public sealed class GetTransformedUrlQuery : IQuery<GetTransformedUrlResponse>
{
    public required string Url { get; init; }
    public int? Width { get; init; }
    public int? Height { get; init; }
    public string? Format { get; init; }
}

public sealed class GetTransformedUrlResponse
{
    public string Url { get; init; } = null!;
}

