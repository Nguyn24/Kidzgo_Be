using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Storage;
using Kidzgo.Application.TeachingMaterials.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.TeachingMaterials;
using Kidzgo.Domain.TeachingMaterials.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TeachingMaterials.GetTeachingMaterialPreviewPdf;

public sealed class GetTeachingMaterialPreviewPdfQueryHandler(
    IDbContext context,
    ITeachingMaterialStorageService storageService,
    ITeachingMaterialPreviewService previewService,
    IUserContext userContext
) : IQueryHandler<GetTeachingMaterialPreviewPdfQuery, GetTeachingMaterialPreviewPdfResponse>
{
    public async Task<Result<GetTeachingMaterialPreviewPdfResponse>> Handle(
        GetTeachingMaterialPreviewPdfQuery query,
        CancellationToken cancellationToken)
    {
        var material = await context.TeachingMaterials
            .FirstOrDefaultAsync(tm => tm.Id == query.TeachingMaterialId, cancellationToken);

        if (material is null)
        {
            return Result.Failure<GetTeachingMaterialPreviewPdfResponse>(
                TeachingMaterialErrors.NotFound(query.TeachingMaterialId));
        }

        if (!await TeachingMaterialAccessHelper.CanReadAsync(context, userContext, material, cancellationToken))
        {
            return Result.Failure<GetTeachingMaterialPreviewPdfResponse>(
                TeachingMaterialErrors.AccessDenied(query.TeachingMaterialId));
        }

        if (!RequiresPdfConversion(material.FileType))
        {
            if (!CanProxyOriginalPreview(material.FileType))
            {
                return Result.Failure<GetTeachingMaterialPreviewPdfResponse>(
                    TeachingMaterialErrors.PdfConversionNotSupported(material.FileType.ToString()));
            }

            var original = await storageService.ReadDecryptedAsync(
                material.StoragePath,
                material.OriginalFileName,
                material.MimeType,
                cancellationToken);

            if (original is null)
            {
                return Result.Failure<GetTeachingMaterialPreviewPdfResponse>(
                    TeachingMaterialErrors.StoredFileMissing(query.TeachingMaterialId));
            }

            return new GetTeachingMaterialPreviewPdfResponse
            {
                FileName = original.FileName,
                MimeType = original.MimeType,
                Content = original.Content
            };
        }

        if (!string.IsNullOrWhiteSpace(material.PdfPreviewPath))
        {
            var cached = await storageService.ReadCacheFileAsync(
                material.PdfPreviewPath,
                "application/pdf",
                Path.ChangeExtension(material.OriginalFileName, ".pdf"),
                cancellationToken);

            if (cached is not null)
            {
                return new GetTeachingMaterialPreviewPdfResponse
                {
                    FileName = cached.FileName,
                    MimeType = cached.MimeType,
                    Content = cached.Content
                };
            }

            material.ClearPdfPreview();
        }

        var file = await storageService.ReadDecryptedAsync(
            material.StoragePath,
            material.OriginalFileName,
            material.MimeType,
            cancellationToken);

        if (file is null)
        {
            return Result.Failure<GetTeachingMaterialPreviewPdfResponse>(
                TeachingMaterialErrors.StoredFileMissing(query.TeachingMaterialId));
        }

        try
        {
            var preview = await previewService.GeneratePdfPreviewAsync(
                material.Id,
                material.OriginalFileName,
                file.Content,
                cancellationToken);

            material.SetPdfPreview(preview.CachePath, preview.FileSize);
            await context.SaveChangesAsync(cancellationToken);

            return new GetTeachingMaterialPreviewPdfResponse
            {
                FileName = Path.ChangeExtension(material.OriginalFileName, ".pdf"),
                MimeType = "application/pdf",
                Content = preview.Content
            };
        }
        catch
        {
            return Result.Failure<GetTeachingMaterialPreviewPdfResponse>(
                TeachingMaterialErrors.PdfConversionFailed(material.Id));
        }
    }

    private static bool RequiresPdfConversion(TeachingMaterialFileType fileType) =>
        fileType is TeachingMaterialFileType.Presentation or TeachingMaterialFileType.Document or TeachingMaterialFileType.Spreadsheet;

    private static bool CanProxyOriginalPreview(TeachingMaterialFileType fileType) =>
        fileType is TeachingMaterialFileType.Image or TeachingMaterialFileType.Pdf or TeachingMaterialFileType.Audio or TeachingMaterialFileType.Video;
}
