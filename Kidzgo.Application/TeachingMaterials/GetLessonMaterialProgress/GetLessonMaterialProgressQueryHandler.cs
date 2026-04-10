using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.TeachingMaterials.Shared;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.TeachingMaterials.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TeachingMaterials.GetLessonMaterialProgress;

public sealed class GetLessonMaterialProgressQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetLessonMaterialProgressQuery, GetLessonMaterialProgressResponse>
{
    public async Task<Result<GetLessonMaterialProgressResponse>> Handle(
        GetLessonMaterialProgressQuery query,
        CancellationToken cancellationToken)
    {
        var role = await TeachingMaterialAccessHelper.GetCurrentUserRoleAsync(context, userContext, cancellationToken);
        if (role is not (UserRole.Admin or UserRole.Teacher))
        {
            return Result.Failure<GetLessonMaterialProgressResponse>(
                TeachingMaterialErrors.AccessDenied(Guid.Empty));
        }

        var materials = await context.TeachingMaterials
            .AsNoTracking()
            .Include(material => material.Program)
            .Where(material =>
                material.ProgramId == query.ProgramId &&
                material.UnitNumber == query.UnitNumber &&
                material.LessonNumber == query.LessonNumber)
            .ToListAsync(cancellationToken);

        if (materials.Count == 0)
        {
            return Result.Failure<GetLessonMaterialProgressResponse>(
                TeachingMaterialErrors.LessonBundleNotFound(query.ProgramId, query.UnitNumber, query.LessonNumber));
        }

        var materialIds = materials.Select(material => material.Id).ToList();
        var progressRows = await context.TeachingMaterialViewProgresses
            .AsNoTracking()
            .Where(progress => materialIds.Contains(progress.TeachingMaterialId))
            .ToListAsync(cancellationToken);

        var students = await context.ClassEnrollments
            .AsNoTracking()
            .Where(enrollment =>
                enrollment.Class.ProgramId == query.ProgramId &&
                enrollment.Status == EnrollmentStatus.Active)
            .Select(enrollment => new
            {
                enrollment.StudentProfile.UserId,
                UserName = enrollment.StudentProfile.DisplayName
            })
            .Distinct()
            .ToListAsync(cancellationToken);

        var studentResponses = students
            .Select(student =>
            {
                var studentProgress = progressRows
                    .Where(progress => progress.UserId == student.UserId)
                    .ToList();

                return new LessonMaterialStudentProgressDto
                {
                    UserId = student.UserId,
                    UserName = student.UserName,
                    MaterialsViewed = studentProgress.Select(progress => progress.TeachingMaterialId).Distinct().Count(),
                    MaterialsCompleted = studentProgress.Count(progress => progress.Completed),
                    OverallProgress = materials.Count == 0
                        ? 0
                        : Math.Round((decimal)studentProgress.Sum(progress => progress.ProgressPercent) / materials.Count, 2),
                    TotalTimeSeconds = studentProgress.Sum(progress => progress.TotalTimeSeconds)
                };
            })
            .OrderBy(student => student.UserName)
            .ToList();

        return new GetLessonMaterialProgressResponse
        {
            ProgramName = materials[0].Program.Name,
            UnitNumber = query.UnitNumber,
            LessonNumber = query.LessonNumber,
            LessonTitle = materials.Select(material => material.LessonTitle).FirstOrDefault(title => !string.IsNullOrWhiteSpace(title)),
            TotalMaterials = materials.Count,
            Students = studentResponses
        };
    }
}
