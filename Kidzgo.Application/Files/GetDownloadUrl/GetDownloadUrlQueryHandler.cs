using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Application.Abstraction.Storage;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Files.Errors;
using Microsoft.Extensions.Logging;

namespace Kidzgo.Application.Files.GetDownloadUrl;

public sealed class GetDownloadUrlQueryHandler(
    IFileStorageService fileStorageService,
    ILogger<GetDownloadUrlQueryHandler> logger
) : IQueryHandler<GetDownloadUrlQuery, GetDownloadUrlResponse>
{
    public Task<Result<GetDownloadUrlResponse>> Handle(GetDownloadUrlQuery query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(query.Url))
        {
            return Task.FromResult( Result.Failure<GetDownloadUrlResponse>(FileErrors.UrlRequired()));
        }

        try
        {
            var downloadUrl = fileStorageService.GetDownloadUrl(query.Url);

            logger.LogInformation("Generated download URL: {OriginalUrl} -> {DownloadUrl}", query.Url, downloadUrl);

            return Task.FromResult(
                Result.Success(new GetDownloadUrlResponse { Url = downloadUrl }));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating download URL: {Url}", query.Url);
            return Task.FromResult(
                Result.Failure<GetDownloadUrlResponse>(FileErrors.DownloadUrlGenerationFailed(ex.Message)));
        }
    }
}

