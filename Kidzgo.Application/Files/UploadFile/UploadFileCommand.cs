using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Files.UploadFile;

public sealed class UploadFileCommand : ICommand<UploadFileResponse>
{
    public required string FileName { get; init; }
    public required long FileSize { get; init; }
    public required string Folder { get; init; }
    public required string ResourceType { get; init; }
    public string? ContentType { get; init; }
    public bool UpdateUserAvatar { get; init; }
    public bool UpdateProfileAvatar { get; init; }
    public Guid? TargetProfileId { get; init; }
    public Stream FileStream { get; init; } = null!;
}

public sealed class UploadFileResponse
{
    public string Url { get; init; } = null!;
    public string FileName { get; init; } = null!;
    public long Size { get; init; }
    public string Folder { get; init; } = null!;
    public string ResourceType { get; init; } = null!;
}

