using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Services;

/// Service để generate sessions từ schedule pattern cho một Class
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

    /// Generate sessions từ schedule pattern cho Class
    /// Chỉ tạo sessions mới cho future dates, không đè lên sessions đã chỉnh sửa
    /// <param name="classEntity">Class entity</param>
    /// <param name="roomId">Room ID để gán cho các sessions được tạo</param>
    /// <param name="onlyFutureSessions">Nếu true, chỉ generate sessions từ hiện tại trở đi. Nếu false, generate tất cả từ StartDate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result với số lượng sessions đã tạo</returns>
        public async Task<Result<int>> GenerateSessionsFromPatternAsync(
        Class classEntity,
        Guid? roomId = null,
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

        // Lấy thông tin Program để giới hạn tổng số sessions theo Program.TotalSessions (nếu có)
        var program = await _context.Programs
            .FirstOrDefaultAsync(p => p.Id == classEntity.ProgramId, cancellationToken);

        int? maxSessions = program?.TotalSessions;

        var now = DateTime.UtcNow;
        var sessionsToCreate = new List<Session>();

        foreach (var occurrence in occurrences)
        {
            // Nếu onlyFutureSessions = true, chỉ tạo sessions từ hiện tại trở đi
            if (onlyFutureSessions && occurrence < now)
            {
                continue;
            }

            // Nếu có giới hạn tổng số sessions theo Program, dừng nếu đã đủ
            if (maxSessions.HasValue &&
                existingSessions.Count + sessionsToCreate.Count >= maxSessions.Value)
            {
                break;
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

            // Kiểm tra xung đột phòng nếu có roomId
            if (roomId.HasValue)
            {
                var sessionStart = occurrence;
                var sessionEnd = sessionStart.AddMinutes(90); // Default duration
                
                // Kiểm tra xem phòng đã bị chiếm dụng bởi class khác vào thời điểm này chưa
                var roomConflict = await _context.Sessions
                    .Include(s => s.Class)
                    .Where(s => s.ClassId != classEntity.Id && // Class khác
                               s.PlannedRoomId == roomId.Value &&
                               s.Status != SessionStatus.Cancelled &&
                               ((s.PlannedDatetime >= sessionStart && s.PlannedDatetime < sessionEnd) ||
                                (s.ActualDatetime.HasValue && s.ActualDatetime.Value >= sessionStart && s.ActualDatetime.Value < sessionEnd) ||
                                (s.PlannedDatetime <= sessionStart && s.PlannedDatetime.AddMinutes(s.DurationMinutes) > sessionStart)))
                    .Select(s => new { s.Class.Code, s.Class.Title, s.PlannedDatetime })
                    .FirstOrDefaultAsync(cancellationToken);

                if (roomConflict != null)
                {
                    return Result.Failure<int>(Error.Validation(
                        "Session.RoomOccupied",
                        $"Phòng đã bị chiếm dụng bởi lớp '{roomConflict.Code} - {roomConflict.Title}' vào ngày {roomConflict.PlannedDatetime:dd/MM/yyyy HH:mm}"));
                }
            }

            // Tạo session mới
            var session = new Session
            {
                Id = Guid.NewGuid(),
                ClassId = classEntity.Id,
                BranchId = classEntity.BranchId,
                PlannedDatetime = occurrence,
                PlannedRoomId = roomId,
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

