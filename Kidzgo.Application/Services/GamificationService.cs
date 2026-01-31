using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Application.Gamification.AddStars;
using Kidzgo.Application.Gamification.AddXp;
using Kidzgo.Domain.Gamification;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Services;

/// <summary>
/// Service để tự động cộng Stars và XP khi hoàn thành các hoạt động
/// </summary>
public sealed class GamificationService : IGamificationService
{
    private readonly ISender _mediator;
    private readonly IDbContext _context;

    public GamificationService(ISender mediator, IDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    public async Task AddStarsForMissionCompletion(Guid studentProfileId, int starsAmount, Guid missionId, string? reason = null, CancellationToken cancellationToken = default)
    {
        // Get current balance
        var currentBalance = await _context.StarTransactions
            .Where(t => t.StudentProfileId == studentProfileId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => t.BalanceAfter)
            .FirstOrDefaultAsync(cancellationToken);

        var newBalance = currentBalance + starsAmount;

        // Create transaction
        var transaction = new StarTransaction
        {
            Id = Guid.NewGuid(),
            StudentProfileId = studentProfileId,
            Amount = starsAmount,
            Reason = reason ?? "Completed Mission",
            SourceType = StarSourceType.Mission,
            SourceId = missionId,
            BalanceAfter = newBalance,
            CreatedBy = null, // System generated
            CreatedAt = DateTime.UtcNow
        };

        _context.StarTransactions.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddStarsForHomeworkCompletion(Guid studentProfileId, int starsAmount, Guid homeworkId, string? reason = null, CancellationToken cancellationToken = default)
    {
        // Get current balance
        var currentBalance = await _context.StarTransactions
            .Where(t => t.StudentProfileId == studentProfileId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => t.BalanceAfter)
            .FirstOrDefaultAsync(cancellationToken);

        var newBalance = currentBalance + starsAmount;

        // Create transaction
        var transaction = new StarTransaction
        {
            Id = Guid.NewGuid(),
            StudentProfileId = studentProfileId,
            Amount = starsAmount,
            Reason = reason ?? "Completed Homework",
            SourceType = StarSourceType.Homework,
            SourceId = homeworkId,
            BalanceAfter = newBalance,
            CreatedBy = null, // System generated
            CreatedAt = DateTime.UtcNow
        };

        _context.StarTransactions.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddStarsForAttendance(Guid studentProfileId, int starsAmount, Guid attendanceId, string? reason = null, CancellationToken cancellationToken = default)
    {
        // Get current balance
        var currentBalance = await _context.StarTransactions
            .Where(t => t.StudentProfileId == studentProfileId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => t.BalanceAfter)
            .FirstOrDefaultAsync(cancellationToken);

        var newBalance = currentBalance + starsAmount;

        // Create transaction
        var transaction = new StarTransaction
        {
            Id = Guid.NewGuid(),
            StudentProfileId = studentProfileId,
            Amount = starsAmount,
            Reason = reason ?? "Attendance Streak",
            SourceType = StarSourceType.Mission, // Using Mission type for attendance streak
            SourceId = attendanceId,
            BalanceAfter = newBalance,
            CreatedBy = null, // System generated
            CreatedAt = DateTime.UtcNow
        };

        _context.StarTransactions.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddXpForMissionCompletion(Guid studentProfileId, int xpAmount, Guid missionId, string? reason = null, CancellationToken cancellationToken = default)
    {
        var command = new AddXpCommand
        {
            StudentProfileId = studentProfileId,
            Amount = xpAmount,
            Reason = reason ?? "Completed Mission"
        };

        await _mediator.Send(command, cancellationToken);
    }

    public async Task AddXpForAttendance(Guid studentProfileId, int xpAmount, Guid attendanceId, string? reason = null, CancellationToken cancellationToken = default)
    {
        var command = new AddXpCommand
        {
            StudentProfileId = studentProfileId,
            Amount = xpAmount,
            Reason = reason ?? "Attendance Streak"
        };

        await _mediator.Send(command, cancellationToken);
    }
}

