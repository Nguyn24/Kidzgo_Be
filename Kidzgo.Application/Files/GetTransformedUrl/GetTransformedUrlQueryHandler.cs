using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Application.Abstraction.Storage;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Files.Errors;
using Microsoft.Extensions.Logging;

namespace Kidzgo.Application.Files.GetTransformedUrl;

public sealed class GetTransformedUrlQueryHandler(
    IFileStorageService fileStorageService,
    ILogger<GetTransformedUrlQueryHandler> logger
) : IQueryHandler<GetTransformedUrlQuery, GetTransformedUrlResponse>
{
    public Task<Result<GetTransformedUrlResponse>> Handle(GetTransformedUrlQuery query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(query.Url))
        {
            return Task.FromResult(Result.Failure<GetTransformedUrlResponse>(FileErrors.UrlRequired()));
        }

        try
        {
            var transformedUrl = fileStorageService.GetTransformedUrl(
                query.Url,
                query.Width,
                query.Height,
                query.Format);

            logger.LogInformation("Generated transformed URL: {OriginalUrl} -> {TransformedUrl}", query.Url, transformedUrl);

            return Task.FromResult(
                Result.Success(new GetTransformedUrlResponse { Url = transformedUrl }));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating transformed URL: {Url}", query.Url);
            return Task.FromResult(
                Result.Failure<GetTransformedUrlResponse>(FileErrors.TransformationFailed(ex.Message)));
        }
    }
}

