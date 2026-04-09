using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Application.TeachingMaterials.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.TeachingMaterials;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TeachingMaterials.GetTeachingMaterials;

public sealed class GetTeachingMaterialsQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetTeachingMaterialsQuery, GetTeachingMaterialsResponse>
{
    public async Task<Result<GetTeachingMaterialsResponse>> Handle(
        GetTeachingMaterialsQuery query,
        CancellationToken cancellationToken)
    {
        var materialsQuery = context.TeachingMaterials
            .Include(tm => tm.Program)
            .Include(tm => tm.UploadedByUser)
            .AsQueryable();

        materialsQuery = await TeachingMaterialAccessHelper.ApplyReadAccessFilterAsync(
            materialsQuery,
            context,
            userContext,
            cancellationToken);

        if (query.ProgramId.HasValue)
        {
            materialsQuery = materialsQuery.Where(tm => tm.ProgramId == query.ProgramId.Value);
        }

        if (query.UnitNumber.HasValue)
        {
            materialsQuery = materialsQuery.Where(tm => tm.UnitNumber == query.UnitNumber.Value);
        }

        if (query.LessonNumber.HasValue)
        {
            materialsQuery = materialsQuery.Where(tm => tm.LessonNumber == query.LessonNumber.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.FileType) &&
            Enum.TryParse<TeachingMaterialFileType>(query.FileType, ignoreCase: true, out var fileType))
        {
            materialsQuery = materialsQuery.Where(tm => tm.FileType == fileType);
        }

        if (!string.IsNullOrWhiteSpace(query.Category) &&
            Enum.TryParse<TeachingMaterialCategory>(query.Category, ignoreCase: true, out var category))
        {
            materialsQuery = materialsQuery.Where(tm => tm.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var searchTerm = query.SearchTerm.Trim().ToLower();
            materialsQuery = materialsQuery.Where(tm =>
                tm.DisplayName.ToLower().Contains(searchTerm) ||
                tm.OriginalFileName.ToLower().Contains(searchTerm) ||
                (tm.LessonTitle != null && tm.LessonTitle.ToLower().Contains(searchTerm)) ||
                (tm.RelativePath != null && tm.RelativePath.ToLower().Contains(searchTerm)) ||
                tm.Program.Name.ToLower().Contains(searchTerm));
        }

        var totalCount = await materialsQuery.CountAsync(cancellationToken);

        var items = await materialsQuery
            .OrderBy(tm => tm.Program.Name)
            .ThenBy(tm => tm.UnitNumber)
            .ThenBy(tm => tm.LessonNumber)
            .ThenBy(tm => tm.DisplayName)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(tm => new TeachingMaterialDto
            {
                Id = tm.Id,
                ProgramId = tm.ProgramId,
                ProgramName = tm.Program.Name,
                ProgramCode = tm.Program.Code,
                UnitNumber = tm.UnitNumber,
                LessonNumber = tm.LessonNumber,
                LessonTitle = tm.LessonTitle,
                RelativePath = tm.RelativePath,
                DisplayName = tm.DisplayName,
                OriginalFileName = tm.OriginalFileName,
                MimeType = tm.MimeType,
                FileSize = tm.FileSize,
                FileType = tm.FileType.ToString(),
                Category = tm.Category.ToString(),
                UploadedByUserId = tm.UploadedByUserId,
                UploadedByName = tm.UploadedByUser.Name,
                IsEncrypted = tm.IsEncrypted,
                PreviewUrl = $"/api/teaching-materials/{tm.Id}/preview",
                PreviewPdfUrl = $"/api/teaching-materials/{tm.Id}/preview-pdf",
                DownloadUrl = $"/api/teaching-materials/{tm.Id}/download",
                HasPdfPreview = tm.PdfPreviewPath != null,
                CreatedAt = tm.CreatedAt,
                UpdatedAt = tm.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return new GetTeachingMaterialsResponse
        {
            Materials = new Page<TeachingMaterialDto>(items, totalCount, query.PageNumber, query.PageSize)
        };
    }
}
