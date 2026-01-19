using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Services;

/// <summary>
/// Service để generate sessions từ schedule pattern cho một Class
/// </summary>
public sealed class SessionGenerationService
{
    private readonly IDbContext _context;
    private readonly ISchedulePatternParser _patternParser;

    public SessionGenerationService(
        IDbContext context,
        ISchedulePatternParser patternParser)
    {
        _context = context;
        _patternParser = patternParser;
    }

    /// <summary>
    /// Generate sessions từ schedule pattern cho Class
    /// Chỉ tạo sessions mới cho future dates, không đè lên sessions đã chỉnh sửa
    /// </summary>
    /// <param name="classEntity">Class entity</param>
    /// <param name="onlyFutureSessions">Nếu true, chỉ generate sessions từ hiện tại trở đi. Nếu false, generate tất cả từ StartDate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result với số lượng sessions đã tạo</returns>
    public async Task<Result<int>> GenerateSessionsFromPatternAsync(
        Class classEntity,
        bool onlyFutureSessions = true,
        CancellationToken cancellationToken = default)
    {
        // Nếu không có schedule pattern, không làm gì
        if (string.IsNullOrWhiteSpace(classEntity.SchedulePattern))
        {
            return Result.Success(0);
        }

        // Nếu không có EndDate, không thể generate (cần biết khi nào dừng)
        if (!classEntity.EndDate.HasValue)
        {
            return Result.Success(0);
        }

        // Parse pattern và generate occurrences
        var parseResult = _patternParser.ParseAndGenerateOccurrences(
            classEntity.SchedulePattern,
            classEntity.StartDate,
            classEntity.EndDate);

        if (parseResult.IsFailure)
        {
            return Result.Failure<int>(parseResult.Error);
        }

        var occurrences = parseResult.Value;

        if (occurrences.Count == 0)
        {
            return Result.Success(0);
        }

        // Lấy danh sách sessions hiện có của class để check trùng lặp
        var existingSessions = await _context.Sessions
            .Where(s => s.ClassId == classEntity.Id)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var sessionsToCreate = new List<Session>();

        foreach (var occurrence in occurrences)
        {
            // Nếu onlyFutureSessions = true, chỉ tạo sessions từ hiện tại trở đi
            if (onlyFutureSessions && occurrence < now)
            {
                continue;
            }

            // Check xem đã có session với PlannedDatetime này chưa (tolerance 1 phút)
            var existingSession = existingSessions
                .FirstOrDefault(s => Math.Abs((s.PlannedDatetime - occurrence).TotalMinutes) < 1);

            if (existingSession != null)
            {
                // Session đã tồn tại
                // Nếu session đã được chỉnh sửa (có ActualDatetime hoặc Status != Scheduled), không đè lên
                if (existingSession.ActualDatetime.HasValue || 
                    existingSession.Status != SessionStatus.Scheduled)
                {
                    // Session đã được chỉnh sửa (đã complete/cancel hoặc đổi lịch), bỏ qua
                    continue;
                }

                // Session tồn tại nhưng chưa được chỉnh sửa, không tạo duplicate
                continue;
            }

            // Tạo session mới
            var session = new Session
            {
                Id = Guid.NewGuid(),
                ClassId = classEntity.Id,
                BranchId = classEntity.BranchId,
                PlannedDatetime = occurrence,
                PlannedRoomId = null, // Có thể set từ Class defaults sau
                PlannedTeacherId = classEntity.MainTeacherId,
                PlannedAssistantId = classEntity.AssistantTeacherId,
                DurationMinutes = 90, // Default duration, có thể lấy từ Program sau
                ParticipationType = ParticipationType.Main,
                Status = SessionStatus.Scheduled,
                CreatedAt = now,
                UpdatedAt = now
            };

            sessionsToCreate.Add(session);
        }

        if (sessionsToCreate.Count > 0)
        {
            _context.Sessions.AddRange(sessionsToCreate);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Result.Success(sessionsToCreate.Count);
    }
}

