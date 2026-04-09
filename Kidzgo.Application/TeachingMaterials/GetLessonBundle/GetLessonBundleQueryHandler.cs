using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.TeachingMaterials.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.TeachingMaterials;
using Kidzgo.Domain.TeachingMaterials.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TeachingMaterials.GetLessonBundle;

public sealed class GetLessonBundleQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetLessonBundleQuery, GetLessonBundleResponse>
{
    public async Task<Result<GetLessonBundleResponse>> Handle(
        GetLessonBundleQuery query,
        CancellationToken cancellationToken)
    {
        var materialsQuery = context.TeachingMaterials
            .AsNoTracking()
            .Include(tm => tm.Program)
            .Where(tm => tm.ProgramId == query.ProgramId &&
                         tm.UnitNumber == query.UnitNumber &&
                         tm.LessonNumber == query.LessonNumber);

        materialsQuery = await TeachingMaterialAccessHelper.ApplyReadAccessFilterAsync(
            materialsQuery,
            context,
            userContext,
            cancellationToken);

        var materials = await materialsQuery
            .OrderBy(tm => tm.RelativePath)
            .ThenBy(tm => tm.DisplayName)
            .ToListAsync(cancellationToken);

        if (materials.Count == 0)
        {
            return Result.Failure<GetLessonBundleResponse>(
                TeachingMaterialErrors.LessonBundleNotFound(query.ProgramId, query.UnitNumber, query.LessonNumber));
        }

        var bundleItems = materials
            .Select(MapItem)
            .ToList();

        var presentations = bundleItems
            .Where(item => item.FileType.Equals(TeachingMaterialFileType.Presentation.ToString(), StringComparison.OrdinalIgnoreCase))
            .ToList();

        var audioFiles = bundleItems
            .Where(item => item.FileType.Equals(TeachingMaterialFileType.Audio.ToString(), StringComparison.OrdinalIgnoreCase))
            .ToList();

        var imageFiles = bundleItems
            .Where(item => item.FileType.Equals(TeachingMaterialFileType.Image.ToString(), StringComparison.OrdinalIgnoreCase))
            .ToList();

        var videoFiles = bundleItems
            .Where(item => item.FileType.Equals(TeachingMaterialFileType.Video.ToString(), StringComparison.OrdinalIgnoreCase))
            .ToList();

        var documents = bundleItems
            .Where(item =>
                item.FileType.Equals(TeachingMaterialFileType.Pdf.ToString(), StringComparison.OrdinalIgnoreCase) ||
                item.FileType.Equals(TeachingMaterialFileType.Document.ToString(), StringComparison.OrdinalIgnoreCase) ||
                item.FileType.Equals(TeachingMaterialFileType.Spreadsheet.ToString(), StringComparison.OrdinalIgnoreCase))
            .ToList();

        var supplementaryFiles = bundleItems
            .Where(item => item.Category.Equals(TeachingMaterialCategory.Supplementary.ToString(), StringComparison.OrdinalIgnoreCase))
            .ToList();

        var otherFiles = bundleItems
            .Where(item =>
                !item.FileType.Equals(TeachingMaterialFileType.Presentation.ToString(), StringComparison.OrdinalIgnoreCase) &&
                !item.FileType.Equals(TeachingMaterialFileType.Audio.ToString(), StringComparison.OrdinalIgnoreCase) &&
                !item.FileType.Equals(TeachingMaterialFileType.Image.ToString(), StringComparison.OrdinalIgnoreCase) &&
                !item.FileType.Equals(TeachingMaterialFileType.Video.ToString(), StringComparison.OrdinalIgnoreCase) &&
                !item.FileType.Equals(TeachingMaterialFileType.Pdf.ToString(), StringComparison.OrdinalIgnoreCase) &&
                !item.FileType.Equals(TeachingMaterialFileType.Document.ToString(), StringComparison.OrdinalIgnoreCase) &&
                !item.FileType.Equals(TeachingMaterialFileType.Spreadsheet.ToString(), StringComparison.OrdinalIgnoreCase))
            .ToList();

        var response = new GetLessonBundleResponse
        {
            ProgramId = materials[0].ProgramId,
            ProgramName = materials[0].Program.Name,
            ProgramCode = materials[0].Program.Code,
            UnitNumber = query.UnitNumber,
            LessonNumber = query.LessonNumber,
            LessonTitle = materials
                .Select(tm => tm.LessonTitle)
                .FirstOrDefault(title => !string.IsNullOrWhiteSpace(title)),
            PrimaryPresentation = presentations.FirstOrDefault(),
            Presentations = presentations,
            AudioFiles = audioFiles,
            ImageFiles = imageFiles,
            VideoFiles = videoFiles,
            Documents = documents,
            SupplementaryFiles = supplementaryFiles,
            OtherFiles = otherFiles
        };

        return response;
    }

    private static TeachingMaterialBundleItem MapItem(TeachingMaterial material)
    {
        return new TeachingMaterialBundleItem
        {
            Id = material.Id,
            DisplayName = material.DisplayName,
            OriginalFileName = material.OriginalFileName,
            RelativePath = material.RelativePath,
            MimeType = material.MimeType,
            FileExtension = material.FileExtension,
            FileSize = material.FileSize,
            FileType = material.FileType.ToString(),
            Category = material.Category.ToString(),
            PreviewUrl = $"/api/teaching-materials/{material.Id}/preview",
            PreviewPdfUrl = $"/api/teaching-materials/{material.Id}/preview-pdf",
            DownloadUrl = $"/api/teaching-materials/{material.Id}/download",
            HasPdfPreview = material.PdfPreviewPath != null,
            CreatedAt = material.CreatedAt
        };
    }
}
