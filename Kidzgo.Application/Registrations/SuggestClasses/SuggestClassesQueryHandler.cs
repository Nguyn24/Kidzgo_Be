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
            .Include(r => r.SecondaryProgram)
            .Include(r => r.Branch)
            .FirstOrDefaultAsync(r => r.Id == query.RegistrationId, cancellationToken);

        if (registration == null)
        {
            return Result.Failure<SuggestClassesResponse>(RegistrationErrors.NotFound(query.RegistrationId));
        }

        var primarySuggestions = await BuildSuggestionsAsync(
            registration.ProgramId,
            registration.BranchId,
            registration.PreferredSchedule,
            cancellationToken);

        var secondarySuggestions = registration.SecondaryProgramId.HasValue
            ? await BuildSuggestionsAsync(
                registration.SecondaryProgramId.Value,
                registration.BranchId,
                registration.PreferredSchedule,
                cancellationToken)
            : (Suggested: new List<SuggestedClassDto>(), Alternative: new List<SuggestedClassDto>());

        return new SuggestClassesResponse
        {
            RegistrationId = registration.Id,
            ProgramName = registration.Program.Name,
            BranchName = registration.Branch.Name,
            PreferredSchedule = registration.PreferredSchedule,
            SuggestedClasses = primarySuggestions.Suggested,
            AlternativeClasses = primarySuggestions.Alternative,
            SecondaryProgramId = registration.SecondaryProgramId,
            SecondaryProgramName = registration.SecondaryProgram?.Name,
            SecondaryProgramSkillFocus = registration.SecondaryProgramSkillFocus,
            SecondarySuggestedClasses = secondarySuggestions.Suggested,
            SecondaryAlternativeClasses = secondarySuggestions.Alternative
        };
    }

    private async Task<(List<SuggestedClassDto> Suggested, List<SuggestedClassDto> Alternative)> BuildSuggestionsAsync(
        Guid programId,
        Guid branchId,
        string? preferredSchedule,
        CancellationToken cancellationToken)
    {
        var matchingClasses = await context.Classes
            .Include(c => c.MainTeacher)
            .Include(c => c.ClassEnrollments)
            .Where(c => c.ProgramId == programId
                && c.BranchId == branchId
                && (c.Status == ClassStatus.Recruiting || c.Status == ClassStatus.Active || c.Status == ClassStatus.Planned || c.Status == ClassStatus.Full)
                && c.Capacity > c.ClassEnrollments.Count(ce => ce.Status == EnrollmentStatus.Active))
            .OrderBy(c => c.StartDate)
            .ToListAsync(cancellationToken);

        var normalizedPreferredSchedule = preferredSchedule?.ToLowerInvariant() ?? string.Empty;
        var filteredClasses = matchingClasses
            .Where(c => IsScheduleMatching(normalizedPreferredSchedule, c.SchedulePattern))
            .ToList();

        var now = DateOnly.FromDateTime(DateTime.UtcNow);

        return (
            filteredClasses
                .Where(c => c.StartDate <= now.AddDays(7))
                .Select(c => MapSuggestedClass(c, now))
                .ToList(),
            filteredClasses
                .Where(c => c.StartDate > now.AddDays(7))
                .Select(c => MapSuggestedClass(c, now))
                .ToList());
    }

    private static SuggestedClassDto MapSuggestedClass(Kidzgo.Domain.Classes.Class classEntity, DateOnly now)
    {
        var currentEnrollment = classEntity.ClassEnrollments.Count(ce => ce.Status == EnrollmentStatus.Active);
        var status = classEntity.Status == ClassStatus.Full && currentEnrollment < classEntity.Capacity
            ? (classEntity.StartDate <= now ? ClassStatus.Active : ClassStatus.Recruiting)
            : classEntity.Status;

        return new SuggestedClassDto
        {
            Id = classEntity.Id,
            Code = classEntity.Code,
            Title = classEntity.Title,
            Status = status.ToString(),
            Capacity = classEntity.Capacity,
            CurrentEnrollment = currentEnrollment,
            StartDate = classEntity.StartDate,
            EndDate = classEntity.EndDate,
            SchedulePattern = classEntity.SchedulePattern,
            MainTeacherName = classEntity.MainTeacher != null ? classEntity.MainTeacher.Name : "Not assigned",
            ClassroomName = null,
            IsClassStarted = classEntity.StartDate <= now
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
