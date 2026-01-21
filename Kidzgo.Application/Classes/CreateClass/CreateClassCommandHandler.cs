using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
// using Kidzgo.Application.Services; // TODO: Uncomment after Services namespace is fixed
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Kidzgo.Application.Classes.CreateClass;

public sealed class CreateClassCommandHandler(
    IDbContext context
    // SessionGenerationService sessionGenerationService // TODO: Uncomment after Services namespace is fixed
) : ICommandHandler<CreateClassCommand, CreateClassResponse>
{
    public async Task<Result<CreateClassResponse>> Handle(CreateClassCommand command, CancellationToken cancellationToken)
    {
        // Check if branch exists
        bool branchExists = await context.Branches
            .AnyAsync(b => b.Id == command.BranchId && b.IsActive, cancellationToken);

        if (!branchExists)
        {
            return Result.Failure<CreateClassResponse>(
                ClassErrors.BranchNotFound);
        }

        // Check if program exists and get program details
        var program = await context.Programs
            .FirstOrDefaultAsync(p => p.Id == command.ProgramId && !p.IsDeleted && p.IsActive, cancellationToken);

        if (program is null)
        {
            return Result.Failure<CreateClassResponse>(
                ClassErrors.ProgramNotFound);
        }

        // Check if code is unique
        bool codeExists = await context.Classes
            .AnyAsync(c => c.Code == command.Code, cancellationToken);

        if (codeExists)
        {
            return Result.Failure<CreateClassResponse>(
                ClassErrors.CodeExists);
        }

        // Check if teachers exist, are TEACHER role, and belong to the same branch
        if (command.MainTeacherId.HasValue)
        {
            var mainTeacher = await context.Users
                .FirstOrDefaultAsync(u => u.Id == command.MainTeacherId.Value && u.Role == Domain.Users.UserRole.Teacher, cancellationToken);

            if (mainTeacher is null)
            {
                return Result.Failure<CreateClassResponse>(
                    ClassErrors.MainTeacherNotFound);
            }

            // Check if teacher belongs to the same branch as the class
            if (mainTeacher.BranchId != command.BranchId)
            {
                return Result.Failure<CreateClassResponse>(
                    ClassErrors.MainTeacherBranchMismatch);
            }
        }

        if (command.AssistantTeacherId.HasValue)
        {
            var assistantTeacher = await context.Users
                .FirstOrDefaultAsync(u => u.Id == command.AssistantTeacherId.Value && u.Role == Domain.Users.UserRole.Teacher, cancellationToken);

            if (assistantTeacher is null)
            {
                return Result.Failure<CreateClassResponse>(
                    ClassErrors.AssistantTeacherNotFound);
            }

            // Check if teacher belongs to the same branch as the class
            if (assistantTeacher.BranchId != command.BranchId)
            {
                return Result.Failure<CreateClassResponse>(
                    ClassErrors.AssistantTeacherBranchMismatch);
            }
        }

        // Tính EndDate tự động nếu không được cung cấp và có SchedulePattern + TotalSessions
        var endDate = command.EndDate;
        
        // Chỉ tính tự động nếu EndDate không được cung cấp (null)
        // và có đủ thông tin: SchedulePattern và TotalSessions > 0
        // Kiểm tra cả điều kiện: EndDate null HOẶC không có giá trị hợp lệ
        if (!endDate.HasValue && 
            !string.IsNullOrWhiteSpace(command.SchedulePattern) && 
            program.TotalSessions > 0)
        {
            var calculatedEndDate = CalculateEndDateFromSchedulePattern(
                command.StartDate,
                command.SchedulePattern,
                program.TotalSessions);
            
            if (calculatedEndDate.HasValue)
            {
                endDate = calculatedEndDate;
            }
            // Nếu không tính được, endDate vẫn là null (giữ nguyên giá trị ban đầu)
        }

        var now = DateTime.UtcNow;
        var classEntity = new Class
        {
            Id = Guid.NewGuid(),
            BranchId = command.BranchId,
            ProgramId = command.ProgramId,
            Code = command.Code,
            Title = command.Title,
            MainTeacherId = command.MainTeacherId,
            AssistantTeacherId = command.AssistantTeacherId,
            StartDate = command.StartDate,
            EndDate = endDate,
            Status = ClassStatus.Planned, // Mặc định PLANNED
            Capacity = command.Capacity,
            SchedulePattern = command.SchedulePattern,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Classes.Add(classEntity);
        await context.SaveChangesAsync(cancellationToken);
        
        return new CreateClassResponse
        {
            Id = classEntity.Id,
            BranchId = classEntity.BranchId,
            ProgramId = classEntity.ProgramId,
            Code = classEntity.Code,
            Title = classEntity.Title,
            MainTeacherId = classEntity.MainTeacherId,
            AssistantTeacherId = classEntity.AssistantTeacherId,
            StartDate = classEntity.StartDate,
            EndDate = classEntity.EndDate,
            Status = classEntity.Status.ToString(),
            Capacity = classEntity.Capacity,
            SchedulePattern = classEntity.SchedulePattern
        };
    }

    /// <summary>
    /// Tính EndDate tự động dựa trên StartDate, SchedulePattern và TotalSessions
    /// </summary>
    private static DateOnly? CalculateEndDateFromSchedulePattern(
        DateOnly startDate,
        string schedulePattern,
        int totalSessions)
    {
        if (string.IsNullOrWhiteSpace(schedulePattern))
        {
            return null;
        }

        if (totalSessions <= 0)
        {
            return null;
        }

        try
        {
            // Parse RRULE để lấy số sessions mỗi tuần
            var pattern = schedulePattern.Trim();
            if (!pattern.StartsWith("RRULE:", StringComparison.OrdinalIgnoreCase))
            {
                pattern = "RRULE:" + pattern;
            }

            // Tách DURATION ra khỏi pattern (DURATION không phải là parameter hợp lệ của RRULE)
            var patternWithoutDuration = RemoveDurationParameter(pattern);
            
            // Nếu sau khi remove DURATION mà pattern rỗng hoặc chỉ còn "RRULE:", không thể tính
            if (string.IsNullOrWhiteSpace(patternWithoutDuration) || 
                patternWithoutDuration.Trim().Equals("RRULE:", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            // Parse BYDAY để đếm số ngày trong tuần
            // CalculateSessionsPerWeek sẽ tự xử lý "RRULE:" prefix
            var sessionsPerWeek = CalculateSessionsPerWeek(patternWithoutDuration);
            
            if (sessionsPerWeek <= 0)
            {
                // Không thể tính được số sessions mỗi tuần
                return null;
            }

            // Tính số tuần cần thiết để đạt đủ TotalSessions
            var weeksNeeded = (int)Math.Ceiling((double)totalSessions / sessionsPerWeek);
            
            // Tính EndDate: StartDate + số tuần cần thiết
            // Thêm thêm một vài ngày buffer để đảm bảo đủ sessions (vì tuần đầu/cuối có thể không đủ)
            // Ví dụ: Nếu cần 10 tuần, thêm 6 ngày buffer để đảm bảo có đủ sessions
            var bufferDays = Math.Max(3, sessionsPerWeek); // Buffer ít nhất bằng số sessions/tuần
            var endDate = startDate.AddDays(weeksNeeded * 7 + bufferDays);
            
            return endDate;
        }
        catch (Exception ex)
        {
            // Nếu có lỗi khi parse, trả về null (không tự động tính)
            // Có thể log exception ở đây nếu cần: Console.WriteLine($"Error calculating EndDate: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Tính số sessions mỗi tuần từ RRULE pattern bằng cách đếm BYDAY
    /// </summary>
    private static int CalculateSessionsPerWeek(string rrulePattern)
    {
        if (string.IsNullOrWhiteSpace(rrulePattern))
        {
            return 0;
        }

        try
        {
            // Remove "RRULE:" prefix nếu có
            var pattern = rrulePattern.Trim();
            if (pattern.StartsWith("RRULE:", StringComparison.OrdinalIgnoreCase))
            {
                pattern = pattern.Substring(6);
            }

            // Tìm BYDAY parameter
            var parameters = pattern.Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (var param in parameters)
            {
                var parts = param.Split('=', 2);
                if (parts.Length == 2 && 
                    parts[0].Trim().Equals("BYDAY", StringComparison.OrdinalIgnoreCase))
                {
                    // Đếm số ngày trong BYDAY (ví dụ: "MO,WE,FR" = 3 sessions)
                    var days = parts[1].Trim().Split(',', StringSplitOptions.RemoveEmptyEntries);
                    return days.Length;
                }
            }

            // Nếu không có BYDAY, kiểm tra FREQ
            // Nếu FREQ=DAILY thì 7 sessions/tuần
            foreach (var param in parameters)
            {
                var parts = param.Split('=', 2);
                if (parts.Length == 2 && 
                    parts[0].Trim().Equals("FREQ", StringComparison.OrdinalIgnoreCase))
                {
                    var freq = parts[1].Trim().ToUpperInvariant();
                    if (freq == "DAILY")
                    {
                        return 7;
                    }
                    else if (freq == "WEEKLY")
                    {
                        // Nếu WEEKLY nhưng không có BYDAY, mặc định 1 session/tuần
                        return 1;
                    }
                }
            }

            return 0;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Remove DURATION parameter từ RRULE pattern
    /// </summary>
    private static string RemoveDurationParameter(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            return pattern;
        }

        var hasPrefix = pattern.StartsWith("RRULE:", StringComparison.OrdinalIgnoreCase);
        var patternWithoutPrefix = hasPrefix ? pattern.Substring(6) : pattern;

        var parameters = patternWithoutPrefix.Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Where(p => !p.Trim().StartsWith("DURATION=", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var cleanedPattern = string.Join(";", parameters);
        
        if (hasPrefix && !string.IsNullOrWhiteSpace(cleanedPattern))
        {
            cleanedPattern = "RRULE:" + cleanedPattern;
        }
        else if (hasPrefix && string.IsNullOrWhiteSpace(cleanedPattern))
        {
            cleanedPattern = "RRULE:";
        }

        return cleanedPattern;
    }
}
