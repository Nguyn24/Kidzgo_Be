using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Storage;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Files.Errors;
using Microsoft.Extensions.Logging;

namespace Kidzgo.Application.Files.DeleteFile;

public sealed class DeleteFileCommandHandler(
    IFileStorageService fileStorageService,
    ILogger<DeleteFileCommandHandler> logger
) : ICommandHandler<DeleteFileCommand, DeleteFileResponse>
{
    public async Task<Result<DeleteFileResponse>> Handle(DeleteFileCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(command.Url))
        {
            return Result.Failure<DeleteFileResponse>(FileErrors.UrlRequired());
        }

        try
        {
            var deleted = await fileStorageService.DeleteFileAsync(command.Url, cancellationToken);

            if (deleted)
            {
                logger.LogInformation("File deleted successfully: {Url}", command.Url);
                return Result.Success(new DeleteFileResponse
                {
                    Success = true,
                    Message = "File deleted successfully"
                });
            }

            logger.LogWarning("File not found for deletion: {Url}", command.Url);
            return Result.Failure<DeleteFileResponse>(FileErrors.FileNotFound(command.Url));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting file: {Url}", command.Url);
            return Result.Failure<DeleteFileResponse>(FileErrors.DeleteFailed(ex.Message));
        }
    }
}

