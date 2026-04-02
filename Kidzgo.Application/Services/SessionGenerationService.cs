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

public sealed class SessionGenerationService
{
    private readonly IDbContext _context;
    private readonly ISchedulePatternParser _patternParser;
    private readonly StudentSessionAssignmentService _studentSessionAssignmentService;
    private readonly SessionConflictChecker _conflictChecker;

    public SessionGenerationService(
        IDbContext context,
        ISchedulePatternParser patternParser,
        StudentSessionAssignmentService studentSessionAssignmentService,
        SessionConflictChecker conflictChecker)
    {
        _context = context;
        _patternParser = patternParser;
        _studentSessionAssignmentService = studentSessionAssignmentService;
        _conflictChecker = conflictChecker;
    }

    public async Task<Result<int>> GenerateSessionsFromPatternAsync(
        Class classEntity,
        Guid? roomId = null,
        bool onlyFutureSessions = true,
        CancellationToken cancellationToken = default)
    {
        if (classEntity.Status is not ClassStatus.Planned and not ClassStatus.Active)
        {
            return Result.Failure<int>(SessionErrors.InvalidClassStatus);
        }

        if (string.IsNullOrWhiteSpace(classEntity.SchedulePattern))
        {
            return Result.Failure<int>(SessionErrors.MissingSchedulePattern(classEntity.Id));
        }

        if (!classEntity.EndDate.HasValue)
        {
            return Result.Failure<int>(SessionErrors.MissingClassEndDate(classEntity.Id));
        }

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

        var durationMinutes = _patternParser.ParseDuration(classEntity.SchedulePattern) ?? 90;
        if (durationMinutes <= 0)
        {
            return Result.Failure<int>(SessionErrors.InvalidDuration(durationMinutes));
        }

        var branchExists = await _context.Branches
            .AnyAsync(b => b.Id == classEntity.BranchId && b.IsActive, cancellationToken);
        if (!branchExists)
        {
            return Result.Failure<int>(SessionErrors.InvalidBranch(classEntity.BranchId));
        }

        if (roomId.HasValue)
        {
            var roomExists = await _context.Classrooms
                .AnyAsync(r => r.Id == roomId.Value && r.BranchId == classEntity.BranchId, cancellationToken);
            if (!roomExists)
            {
                return Result.Failure<int>(SessionErrors.InvalidRoom(roomId.Value));
            }
        }

        if (classEntity.MainTeacherId.HasValue)
        {
            var teacherExists = await _context.Users
                .AnyAsync(
                    u => u.Id == classEntity.MainTeacherId.Value &&
                         u.Role == UserRole.Teacher &&
                         u.BranchId == classEntity.BranchId,
                    cancellationToken);
            if (!teacherExists)
            {
                return Result.Failure<int>(SessionErrors.InvalidTeacher(classEntity.MainTeacherId.Value));
            }
        }

        if (classEntity.AssistantTeacherId.HasValue)
        {
            var assistantExists = await _context.Users
                .AnyAsync(
                    u => u.Id == classEntity.AssistantTeacherId.Value &&
                         u.Role == UserRole.Teacher &&
                         u.BranchId == classEntity.BranchId,
                    cancellationToken);
            if (!assistantExists)
            {
                return Result.Failure<int>(SessionErrors.InvalidAssistant(classEntity.AssistantTeacherId.Value));
            }
        }

        var existingSessions = await _context.Sessions
            .Where(s => s.ClassId == classEntity.Id)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var sessionsToCreate = new List<Session>();

        foreach (var occurrence in occurrences)
        {
            if (onlyFutureSessions && occurrence < now)
            {
                continue;
            }

            var existingSession = existingSessions
                .FirstOrDefault(s => Math.Abs((s.PlannedDatetime - occurrence).TotalMinutes) < 1);

            if (existingSession != null)
            {
                continue;
            }

            var conflictResult = await _conflictChecker.CheckConflictsAsync(
                Guid.Empty,
                occurrence,
                durationMinutes,
                roomId,
                classEntity.MainTeacherId,
                classEntity.AssistantTeacherId,
                cancellationToken);

            if (conflictResult.HasConflicts)
            {
                var firstConflict = conflictResult.Conflicts.First();
                return firstConflict.Type switch
                {
                    ConflictType.Room => Result.Failure<int>(
                        ClassErrors.RoomConflict(
                            firstConflict.ClassCode,
                            firstConflict.ClassTitle,
                            firstConflict.ConflictDatetime)),
                    ConflictType.Teacher => Result.Failure<int>(
                        ClassErrors.TeacherConflict(
                            firstConflict.ClassCode,
                            firstConflict.ClassTitle,
                            firstConflict.ConflictDatetime,
                            firstConflict.RoomName)),
                    ConflictType.Assistant => Result.Failure<int>(
                        ClassErrors.AssistantConflict(
                            firstConflict.ClassCode,
                            firstConflict.ClassTitle,
                            firstConflict.ConflictDatetime)),
                    _ => Result.Failure<int>(Error.Validation(
                        "Session.ConflictDetected",
                        "A conflicting session was detected during generation"))
                };
            }

            sessionsToCreate.Add(new Session
            {
                Id = Guid.NewGuid(),
                ClassId = classEntity.Id,
                BranchId = classEntity.BranchId,
                Color = SessionColorPalette.GetRandomColor(),
                PlannedDatetime = occurrence,
                PlannedRoomId = roomId,
                PlannedTeacherId = classEntity.MainTeacherId,
                PlannedAssistantId = classEntity.AssistantTeacherId,
                DurationMinutes = durationMinutes,
                ParticipationType = ParticipationType.Main,
                Status = SessionStatus.Scheduled,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        if (sessionsToCreate.Count == 0)
        {
            return Result.Success(0);
        }

        try
        {
            _context.Sessions.AddRange(sessionsToCreate);
            foreach (var session in sessionsToCreate)
            {
                await _studentSessionAssignmentService.SyncAssignmentsForSessionAsync(session, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            var errorMessage = ex.Message;
            var innerException = ex.InnerException;

            if (innerException != null)
            {
                var innerType = innerException.GetType();
                var innerTypeName = innerType.Name;

                if (innerTypeName.Contains("Postgres") || innerTypeName.Contains("Npgsql"))
                {
                    errorMessage = $"{ex.Message} | Inner: {innerException.Message}";

                    var constraintNameProperty = innerType.GetProperty("ConstraintName");
                    if (constraintNameProperty != null)
                    {
                        var constraintName = constraintNameProperty.GetValue(innerException)?.ToString();
                        if (!string.IsNullOrEmpty(constraintName))
                        {
                            errorMessage += $" | Constraint: {constraintName}";

                            if (constraintName.Contains("PlannedRoomId"))
                            {
                                if (roomId.HasValue)
                                {
                                    return Result.Failure<int>(SessionErrors.InvalidRoom(roomId.Value));
                                }

                                errorMessage = "Room with ID does not exist. Please verify roomId.";
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
                $"Unexpected error while saving sessions: {ex.Message} | Type: {ex.GetType().Name} | StackTrace: {stackTrace}"));
        }

        return Result.Success(sessionsToCreate.Count);
    }
}
