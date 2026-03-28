using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Storage;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.TeachingMaterials.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TeachingMaterials.DownloadTeachingMaterial;

public sealed class DownloadTeachingMaterialQueryHandler(
    IDbContext context,
    ITeachingMaterialStorageService storageService
) : IQueryHandler<DownloadTeachingMaterialQuery, DownloadTeachingMaterialResponse>
{
    public async Task<Result<DownloadTeachingMaterialResponse>> Handle(
        DownloadTeachingMaterialQuery query,
        CancellationToken cancellationToken)
    {
        var material = await context.TeachingMaterials
            .FirstOrDefaultAsync(tm => tm.Id == query.TeachingMaterialId, cancellationToken);

        if (material is null)
        {
            return Result.Failure<DownloadTeachingMaterialResponse>(
                TeachingMaterialErrors.NotFound(query.TeachingMaterialId));
        }

        var file = await storageService.ReadDecryptedAsync(
            material.StoragePath,
            material.OriginalFileName,
            material.MimeType,
            cancellationToken);

        if (file is null)
        {
            return Result.Failure<DownloadTeachingMaterialResponse>(
                TeachingMaterialErrors.StoredFileMissing(query.TeachingMaterialId));
        }

        return new DownloadTeachingMaterialResponse
        {
            FileName = file.FileName,
            MimeType = file.MimeType,
            Content = file.Content
        };
    }
}
