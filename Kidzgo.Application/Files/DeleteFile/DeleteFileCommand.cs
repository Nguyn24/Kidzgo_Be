using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Files.DeleteFile;

public sealed class DeleteFileCommand : ICommand<DeleteFileResponse>
{
    public required string Url { get; init; }
}

public sealed class DeleteFileResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = null!;
}
