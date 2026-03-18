using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Registrations.Errors;
using Kidzgo.Domain.Classes;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Registrations.SuggestClasses.Handler;

public sealed class SuggestClassesQueryHandler(
    IDbContext context
) : IQueryHandler<SuggestClassesQuery, SuggestClassesResponse>
{
    public async Task<Result<SuggestClassesResponse>> Handle(
        SuggestClassesQuery query,
        CancellationToken cancellationToken)
    {
        var registration = await context.Registrations
            .Include(r => r.Program)
            .Include(r => r.Branch)
            .FirstOrDefaultAsync(r => r.Id == query.RegistrationId, cancellationToken);

        if (registration == null)
        {
            return Result.Failure<SuggestClassesResponse>(RegistrationErrors.NotFound(query.RegistrationId));
        }

        var matchingClasses = await context.Classes
            .Include(c => c.MainTeacher)
            .Include(c => c.ClassEnrollments)
            .Where(c => c.ProgramId == registration.ProgramId
                && c.BranchId == registration.BranchId
                && (c.Status == ClassStatus.Recruiting || c.Status == ClassStatus.Active || c.Status == ClassStatus.Planned)
                && c.Capacity > c.ClassEnrollments.Count)
            .OrderBy(c => c.StartDate)
            .ToListAsync(cancellationToken);

        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        
        var suggestedClasses = matchingClasses
            .Where(c => c.StartDate <= now.AddDays(7))
            .Select(c => new SuggestedClassDto
            {
                Id = c.Id,
                Code = c.Code,
                Title = c.Title,
                Status = c.Status.ToString(),
                Capacity = c.Capacity,
                CurrentEnrollment = c.ClassEnrollments.Count,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                SchedulePattern = c.SchedulePattern,
                MainTeacherName = c.MainTeacher != null ? c.MainTeacher.Name : "Not assigned",
                ClassroomName = null,
                IsClassStarted = c.StartDate <= now
            })
            .ToList();

        var alternativeClasses = matchingClasses
            .Where(c => c.StartDate > now.AddDays(7))
            .Select(c => new SuggestedClassDto
            {
                Id = c.Id,
                Code = c.Code,
                Title = c.Title,
                Status = c.Status.ToString(),
                Capacity = c.Capacity,
                CurrentEnrollment = c.ClassEnrollments.Count,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                SchedulePattern = c.SchedulePattern,
                MainTeacherName = c.MainTeacher != null ? c.MainTeacher.Name : "Not assigned",
                ClassroomName = null,
                IsClassStarted = c.StartDate <= now
            })
            .ToList();

        return new SuggestClassesResponse
        {
            RegistrationId = registration.Id,
            ProgramName = registration.Program.Name,
            BranchName = registration.Branch.Name,
            PreferredSchedule = registration.PreferredSchedule,
            SuggestedClasses = suggestedClasses,
            AlternativeClasses = alternativeClasses
        };
    }
}
