using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Registrations.Errors;
using Kidzgo.Domain.Classes;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

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

        // Filter by preferred schedule if provided
        var preferredSchedule = registration.PreferredSchedule?.ToLowerInvariant() ?? "";
        var filteredClasses = matchingClasses
            .Where(c => IsScheduleMatching(preferredSchedule, c.SchedulePattern))
            .ToList();

        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        
        var suggestedClasses = filteredClasses
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

        var alternativeClasses = filteredClasses
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

    private bool IsScheduleMatching(string preferredSchedule, string? schedulePattern)
    {
        if (string.IsNullOrEmpty(preferredSchedule) || string.IsNullOrEmpty(schedulePattern))
        {
            return true; // No filter if no preference
        }

        // Parse RRULE to extract time information
        var hourMatch = Regex.Match(schedulePattern, @"BYHOUR=(\d+)", RegexOptions.IgnoreCase);
        var minuteMatch = Regex.Match(schedulePattern, @"BYMINUTE=(\d+)", RegexOptions.IgnoreCase);
        var dayMatch = Regex.Match(schedulePattern, @"BYDAY=([A-Z,]+)", RegexOptions.IgnoreCase);

        int hour = hourMatch.Success ? int.Parse(hourMatch.Groups[1].Value) : -1;
        int minute = minuteMatch.Success ? int.Parse(minuteMatch.Groups[1].Value) : 0;
        string days = dayMatch.Success ? dayMatch.Groups[1].Value.ToUpperInvariant() : "";

        // Check morning (sáng): 6-12
        if (preferredSchedule.Contains("sáng") || preferredSchedule.Contains("morning"))
        {
            if (hour >= 6 && hour < 12) return true;
        }

        // Check afternoon (chiều): 12-18
        if (preferredSchedule.Contains("chiều") || preferredSchedule.Contains("afternoon"))
        {
            if (hour >= 12 && hour < 18) return true;
        }

        // Check evening (tối): 18-22
        if (preferredSchedule.Contains("tối") || preferredSchedule.Contains("tui") || preferredSchedule.Contains("evening"))
        {
            if (hour >= 18 && hour < 22) return true;
        }

        // Check weekend (cuối tuần)
        if (preferredSchedule.Contains("cuối tuần") || preferredSchedule.Contains("cuoi tuan") || 
            preferredSchedule.Contains("weekend") || preferredSchedule.Contains("thứ 7") || preferredSchedule.Contains("cn"))
        {
            if (days.Contains("SA") || days.Contains("SU")) return true;
        }

        // Check weekday (trong tuần)
        if (preferredSchedule.Contains("trong tuần") || preferredSchedule.Contains("trong tuan") || 
            preferredSchedule.Contains("weekday"))
        {
            if (!days.Contains("SA") && !days.Contains("SU")) return true;
        }

        // Check specific days
        if (preferredSchedule.Contains("thứ 2") || preferredSchedule.Contains("mon"))
        {
            if (days.Contains("MO")) return true;
        }
        if (preferredSchedule.Contains("thứ 3") || preferredSchedule.Contains("tue"))
        {
            if (days.Contains("TU")) return true;
        }
        if (preferredSchedule.Contains("thứ 4") || preferredSchedule.Contains("wed"))
        {
            if (days.Contains("WE")) return true;
        }
        if (preferredSchedule.Contains("thứ 5") || preferredSchedule.Contains("thu"))
        {
            if (days.Contains("TH")) return true;
        }
        if (preferredSchedule.Contains("thứ 6") || preferredSchedule.Contains("fri"))
        {
            if (days.Contains("FR")) return true;
        }

        return false;
    }
}
