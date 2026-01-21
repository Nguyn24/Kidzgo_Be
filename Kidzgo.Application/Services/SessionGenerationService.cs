using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Kidzgo.Domain.Users;
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

        // Parse duration từ schedule pattern, fallback về 90 phút nếu không có
        var durationMinutes = _patternParser.ParseDuration(classEntity.SchedulePattern) ?? 90;
        
        // Validate duration phải > 0
        if (durationMinutes <= 0)
        {
            return Result.Failure<int>(SessionErrors.InvalidDuration(durationMinutes));
        }

        // Validate foreign keys tồn tại
        // Kiểm tra Branch tồn tại
        var branchExists = await _context.Branches
            .AnyAsync(b => b.Id == classEntity.BranchId && b.IsActive, cancellationToken);
        if (!branchExists)
        {
            return Result.Failure<int>(SessionErrors.InvalidBranch(classEntity.BranchId));
        }

        // Kiểm tra Room tồn tại nếu có roomId
        if (roomId.HasValue)
        {
            var roomExists = await _context.Classrooms
                .AnyAsync(r => r.Id == roomId.Value && r.BranchId == classEntity.BranchId, cancellationToken);
            if (!roomExists)
            {
                return Result.Failure<int>(SessionErrors.InvalidRoom(roomId.Value));
            }
        }

        // Kiểm tra Teacher tồn tại nếu có
        if (classEntity.MainTeacherId.HasValue)
        {
            var teacherExists = await _context.Users
                .AnyAsync(u => u.Id == classEntity.MainTeacherId.Value && 
                              u.Role == UserRole.Teacher &&
                              u.BranchId == classEntity.BranchId, cancellationToken);
            if (!teacherExists)
            {
                return Result.Failure<int>(SessionErrors.InvalidTeacher(classEntity.MainTeacherId.Value));
            }
        }

        // Kiểm tra Assistant Teacher tồn tại nếu có
        if (classEntity.AssistantTeacherId.HasValue)
        {
            var assistantExists = await _context.Users
                .AnyAsync(u => u.Id == classEntity.AssistantTeacherId.Value && 
                              u.Role == UserRole.Teacher &&
                              u.BranchId == classEntity.BranchId, cancellationToken);
            if (!assistantExists)
            {
                return Result.Failure<int>(SessionErrors.InvalidAssistant(classEntity.AssistantTeacherId.Value));
            }
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
        var skippedFutureCount = 0;
        var skippedDuplicateCount = 0;
        var skippedMaxLimitCount = 0;

        foreach (var occurrence in occurrences)
        {
            // Nếu onlyFutureSessions = true, chỉ tạo sessions từ hiện tại trở đi
            if (onlyFutureSessions && occurrence < now)
            {
                skippedFutureCount++;
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
                    skippedDuplicateCount++;
                    continue;
                }

                // Session tồn tại nhưng chưa được chỉnh sửa, không tạo duplicate
                skippedDuplicateCount++;
                continue;
            }

            // Nếu có giới hạn tổng số sessions theo Program, dừng nếu đã đủ
            // Check TRƯỚC KHI thêm session vào list để đảm bảo không vượt quá giới hạn
            if (maxSessions.HasValue)
            {
                var totalSessionsAfterAdd = existingSessions.Count + sessionsToCreate.Count + 1;
                if (totalSessionsAfterAdd > maxSessions.Value)
                {
                    // Đã đạt giới hạn, không tạo thêm session nào nữa
                    skippedMaxLimitCount++;
                    break;
                }
            }

            // Kiểm tra xung đột phòng nếu có roomId
            if (roomId.HasValue)
            {
                var sessionStart = occurrence;
                var sessionEnd = sessionStart.AddMinutes(durationMinutes);
                
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
                    return Result.Failure<int>(SessionErrors.RoomOccupied(
                        roomConflict.Code,
                        roomConflict.Title,
                        roomConflict.PlannedDatetime));
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
                DurationMinutes = durationMinutes, // Lấy từ DURATION trong schedule pattern, fallback 90 phút
                ParticipationType = ParticipationType.Main,
                Status = SessionStatus.Scheduled,
                CreatedAt = now,
                UpdatedAt = now
            };

            sessionsToCreate.Add(session);
        }

        if (sessionsToCreate.Count > 0)
        {
            try
            {
                _context.Sessions.AddRange(sessionsToCreate);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                // Log chi tiết lỗi để debug
                var errorMessage = ex.Message;
                var innerException = ex.InnerException;
                
                // Lấy chi tiết từ PostgreSQL exception nếu có (sử dụng reflection để tránh dependency Npgsql)
                if (innerException != null)
                {
                    var innerType = innerException.GetType();
                    var innerTypeName = innerType.Name;
                    
                    // Kiểm tra nếu là PostgreSQL exception
                    if (innerTypeName.Contains("Postgres") || innerTypeName.Contains("Npgsql"))
                    {
                        errorMessage = $"{ex.Message} | Inner: {innerException.Message}";
                        
                        // Lấy constraint name bằng reflection
                        var constraintNameProperty = innerType.GetProperty("ConstraintName");
                        if (constraintNameProperty != null)
                        {
                            var constraintName = constraintNameProperty.GetValue(innerException)?.ToString();
                            if (!string.IsNullOrEmpty(constraintName))
                            {
                                errorMessage += $" | Constraint: {constraintName}";
                                
                                // Xử lý các constraint cụ thể
                                if (constraintName.Contains("PlannedRoomId"))
                                {
                                    if (roomId.HasValue)
                                    {
                                        return Result.Failure<int>(SessionErrors.InvalidRoom(roomId.Value));
                                    }
                                    errorMessage = $"Room với ID không tồn tại. Vui lòng kiểm tra lại roomId.";
                                }
                                else if (constraintName.Contains("BranchId"))
                                {
                                    return Result.Failure<int>(SessionErrors.InvalidBranch(classEntity.BranchId));
                                }
                                else if (constraintName.Contains("ClassId"))
                                {
                                    return Result.Failure<int>(ClassErrors.NotFound(classEntity.Id));
                                }
                                else if (constraintName.Contains("PlannedTeacherId") && classEntity.MainTeacherId.HasValue)
                                {
                                    return Result.Failure<int>(SessionErrors.InvalidTeacher(classEntity.MainTeacherId.Value));
                                }
                                else if (constraintName.Contains("PlannedAssistantId") && classEntity.AssistantTeacherId.HasValue)
                                {
                                    return Result.Failure<int>(SessionErrors.InvalidAssistant(classEntity.AssistantTeacherId.Value));
                                }
                            }
                        }
                        
                        // Lấy detail nếu có
                        var detailProperty = innerType.GetProperty("Detail");
                        if (detailProperty != null)
                        {
                            var detail = detailProperty.GetValue(innerException)?.ToString();
                            if (!string.IsNullOrEmpty(detail))
                            {
                                errorMessage += $" | Detail: {detail}";
                            }
                        }
                    }
                    else
                    {
                        errorMessage = $"{ex.Message} | Inner: {innerException.Message}";
                    }
                }
                
                // Log thêm thông tin về entries đang được save
                var entries = ex.Entries?.Select(e => $"{e.Entity.GetType().Name} - {e.State}").ToList();
                if (entries != null && entries.Any())
                {
                    errorMessage += $" | Entries: {string.Join(", ", entries)}";
                }
                
                return Result.Failure<int>(SessionErrors.SaveFailed(errorMessage));
            }
            catch (Exception ex)
            {
                var stackTrace = ex.StackTrace != null 
                    ? ex.StackTrace.Substring(0, Math.Min(500, ex.StackTrace.Length)) 
                    : "";
                
                return Result.Failure<int>(Error.Validation(
                    "Session.SaveFailed",
                    $"Lỗi không mong đợi khi lưu sessions: {ex.Message} | Type: {ex.GetType().Name} | StackTrace: {stackTrace}"));
            }
        }

        return Result.Success(sessionsToCreate.Count);
    }
}

