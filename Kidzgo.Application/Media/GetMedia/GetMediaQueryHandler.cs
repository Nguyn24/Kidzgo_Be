using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Media.GetMedia;

public sealed class GetMediaQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetMediaQuery, GetMediaResponse>
{
    public async Task<Result<GetMediaResponse>> Handle(GetMediaQuery query, CancellationToken cancellationToken)
    {
        // Get StudentId from context (token)
        var studentId = userContext.StudentId;

        if (!studentId.HasValue)
        {
            return Result.Failure<GetMediaResponse>(ProfileErrors.StudentNotFound);
        }

        // Verify the student belongs to the current user
        var profile = await context.Profiles
            .FirstOrDefaultAsync(p => p.Id == studentId.Value && p.UserId == userContext.UserId, cancellationToken);

        if (profile == null || profile.ProfileType != Domain.Users.ProfileType.Student)
        {
            return Result.Failure<GetMediaResponse>(ProfileErrors.StudentNotFound);
        }

        var mediaQuery = context.MediaAssets
            .Include(m => m.UploaderUser)
            .Include(m => m.ApprovedByUser)
            .Include(m => m.Branch)
            .Include(m => m.Class)
            .Include(m => m.StudentProfile)
            .Where(m => !m.IsDeleted)
            .AsQueryable();

        // Filter by branch
        if (query.BranchId.HasValue)
        {
            mediaQuery = mediaQuery.Where(m => m.BranchId == query.BranchId.Value);
        }

        // Filter by class
        if (query.ClassId.HasValue)
        {
            mediaQuery = mediaQuery.Where(m => m.ClassId == query.ClassId.Value);
        }

        // Filter by student from context
        mediaQuery = mediaQuery.Where(m => m.StudentProfileId == studentId.Value);

        // Filter by month tag
        if (!string.IsNullOrWhiteSpace(query.MonthTag))
        {
            mediaQuery = mediaQuery.Where(m => m.MonthTag == query.MonthTag);
        }

        // Filter by type
        if (query.Type.HasValue)
        {
            mediaQuery = mediaQuery.Where(m => m.Type == query.Type.Value);
        }

        // Filter by content type
        if (query.ContentType.HasValue)
        {
            mediaQuery = mediaQuery.Where(m => m.ContentType == query.ContentType.Value);
        }

        // Filter by visibility
        if (query.Visibility.HasValue)
        {
            mediaQuery = mediaQuery.Where(m => m.Visibility == query.Visibility.Value);
        }

        // Filter by approval status
        if (query.ApprovalStatus.HasValue)
        {
            mediaQuery = mediaQuery.Where(m => m.ApprovalStatus == query.ApprovalStatus.Value);
        }

        // Filter by published status
        if (query.IsPublished.HasValue)
        {
            mediaQuery = mediaQuery.Where(m => m.IsPublished == query.IsPublished.Value);
        }

        // Get total count
        int totalCount = await mediaQuery.CountAsync(cancellationToken);

        // Apply pagination and ordering
        var media = await mediaQuery
            .OrderByDescending(m => m.CreatedAt)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(m => new MediaDto
            {
                Id = m.Id,
                UploaderId = m.UploaderId,
                UploaderName = m.UploaderUser.Name,
                BranchId = m.BranchId,
                BranchName = m.Branch.Name,
                ClassId = m.ClassId,
                ClassName = m.Class != null ? m.Class.Title : null,
                StudentProfileId = m.StudentProfileId,
                StudentName = m.StudentProfile != null ? m.StudentProfile.DisplayName : null,
                MonthTag = m.MonthTag,
                Type = m.Type,
                ContentType = m.ContentType,
                Url = m.Url,
                Caption = m.Caption,
                Visibility = m.Visibility,
                ApprovalStatus = m.ApprovalStatus,
                ApprovedById = m.ApprovedById,
                ApprovedByName = m.ApprovedByUser != null ? m.ApprovedByUser.Name : null,
                ApprovedAt = m.ApprovedAt,
                IsPublished = m.IsPublished,
                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        var page = new Page<MediaDto>(
            media,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetMediaResponse
        {
            Media = page
        };
    }
}

