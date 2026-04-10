using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.TeachingMaterials.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.TeachingMaterials.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TeachingMaterials.GetTeachingMaterialById;

public sealed class GetTeachingMaterialByIdQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetTeachingMaterialByIdQuery, GetTeachingMaterialByIdResponse>
{
    public async Task<Result<GetTeachingMaterialByIdResponse>> Handle(
        GetTeachingMaterialByIdQuery query,
        CancellationToken cancellationToken)
    {
        var material = await context.TeachingMaterials
            .Include(tm => tm.Program)
            .Include(tm => tm.UploadedByUser)
            .FirstOrDefaultAsync(tm => tm.Id == query.TeachingMaterialId, cancellationToken);

        if (material is null)
        {
            return Result.Failure<GetTeachingMaterialByIdResponse>(
                TeachingMaterialErrors.NotFound(query.TeachingMaterialId));
        }

        if (!await TeachingMaterialAccessHelper.CanReadAsync(context, userContext, material, cancellationToken))
        {
            return Result.Failure<GetTeachingMaterialByIdResponse>(
                TeachingMaterialErrors.AccessDenied(query.TeachingMaterialId));
        }

        return new GetTeachingMaterialByIdResponse
        {
            Id = material.Id,
            ProgramId = material.ProgramId,
            ProgramName = material.Program.Name,
            ProgramCode = material.Program.Code,
            UnitNumber = material.UnitNumber,
            LessonNumber = material.LessonNumber,
            LessonTitle = material.LessonTitle,
            RelativePath = material.RelativePath,
            DisplayName = material.DisplayName,
            OriginalFileName = material.OriginalFileName,
            MimeType = material.MimeType,
            FileExtension = material.FileExtension,
            FileSize = material.FileSize,
            FileType = material.FileType.ToString(),
            Category = material.Category.ToString(),
            IsEncrypted = material.IsEncrypted,
            EncryptionAlgorithm = material.EncryptionAlgorithm,
            EncryptionKeyVersion = material.EncryptionKeyVersion,
            UploadedByUserId = material.UploadedByUserId,
            UploadedByName = material.UploadedByUser.Name,
            PreviewUrl = $"/api/teaching-materials/{material.Id}/preview",
            PreviewPdfUrl = $"/api/teaching-materials/{material.Id}/preview-pdf",
            DownloadUrl = $"/api/teaching-materials/{material.Id}/download",
            HasPdfPreview = material.PdfPreviewPath != null,
            CreatedAt = material.CreatedAt,
            UpdatedAt = material.UpdatedAt
        };
    }
}
