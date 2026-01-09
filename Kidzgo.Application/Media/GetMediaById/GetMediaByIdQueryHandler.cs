using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Media.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Media.GetMediaById;

public sealed class GetMediaByIdQueryHandler(
    IDbContext context
) : IQueryHandler<GetMediaByIdQuery, GetMediaByIdResponse>
{
    public async Task<Result<GetMediaByIdResponse>> Handle(GetMediaByIdQuery query, CancellationToken cancellationToken)
    {
        var media = await context.MediaAssets
            .Include(m => m.UploaderUser)
            .Include(m => m.ApprovedByUser)
            .Include(m => m.Branch)
            .Include(m => m.Class)
            .Include(m => m.StudentProfile)
            .Where(m => m.Id == query.Id && !m.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (media is null)
        {
            return Result.Failure<GetMediaByIdResponse>(
                MediaErrors.NotFound(query.Id));
        }

        return new GetMediaByIdResponse
        {
            Id = media.Id,
            UploaderId = media.UploaderId,
            UploaderName = media.UploaderUser.Name,
            BranchId = media.BranchId,
            BranchName = media.Branch.Name,
            ClassId = media.ClassId,
            ClassName = media.Class?.Title,
            StudentProfileId = media.StudentProfileId,
            StudentName = media.StudentProfile?.DisplayName,
            MonthTag = media.MonthTag,
            Type = media.Type,
            ContentType = media.ContentType,
            Url = media.Url,
            Caption = media.Caption,
            Visibility = media.Visibility,
            ApprovalStatus = media.ApprovalStatus,
            ApprovedById = media.ApprovedById,
            ApprovedByName = media.ApprovedByUser?.Name,
            ApprovedAt = media.ApprovedAt,
            IsPublished = media.IsPublished,
            CreatedAt = media.CreatedAt,
            UpdatedAt = media.UpdatedAt
        };
    }
}

