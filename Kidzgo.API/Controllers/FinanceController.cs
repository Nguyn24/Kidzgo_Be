using Kidzgo.API.Extensions;
using Kidzgo.API.Infrastructure;
using Kidzgo.API.Requests;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Invoices.CreatePayOSLink;
using Kidzgo.Application.Users.GetAccountantStaffOverview;
using Kidzgo.Domain.Finance;
using Kidzgo.Domain.Payroll;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.API.Controllers;

[Route("api/finance")]
[ApiController]
[Authorize(Roles = "AccountantStaff,Admin,ManagementStaff")]
public class FinanceController : ControllerBase
{
    private readonly IDbContext _context;
    private readonly ISender _mediator;
    private readonly IUserContext _userContext;

    public FinanceController(IDbContext context, ISender mediator, IUserContext userContext)
    {
        _context = context;
        _mediator = mediator;
        _userContext = userContext;
    }

    [HttpGet("accountant/dashboard")]
    public async Task<IResult> GetFinanceAccountantDashboard(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAccountantStaffOverviewQuery(), cancellationToken);
        if (!result.IsSuccess)
        {
            return CustomResults.Problem(result);
        }

        var paymentMethodBreakdown = result.Value.RecentPayments
            .GroupBy(p => p.PaymentMethod)
            .Select(g => new
            {
                method = g.Key,
                count = g.Count(),
                amount = g.Sum(x => x.Amount)
            })
            .ToList();

        return OkData(new
        {
            totalInvoices = result.Value.Statistics.PendingInvoices + result.Value.Statistics.OverdueInvoices,
            overdueAmount = result.Value.Statistics.OverdueAmount,
            transactionHealth = new
            {
                totalRevenue = result.Value.Statistics.TotalRevenue,
                pendingPayments = result.Value.Statistics.PendingPayments,
                cashBalance = result.Value.Statistics.CashBalance
            },
            paymentMethodBreakdown
        });
    }

    [HttpGet("cashbook")]
    public async Task<IResult> GetFinanceCashbook(CancellationToken cancellationToken)
    {
        var rows = await _context.CashbookEntries
            .AsNoTracking()
            .OrderByDescending(cb => cb.EntryDate)
            .Take(200)
            .Select(cb => new
            {
                id = cb.Id,
                type = cb.Type.ToString(),
                title = cb.Description ?? (cb.RelatedType != null ? cb.RelatedType.ToString() : "Cashbook Entry"),
                amount = cb.Amount,
                date = cb.EntryDate,
                method = "Manual",
                note = cb.Description,
                category = cb.RelatedType != null ? cb.RelatedType.ToString() : null,
                branch = cb.Branch.Name,
                status = "Recorded"
            })
            .ToListAsync(cancellationToken);

        return OkData(rows);
    }

    [HttpPost("cashbook")]
    public async Task<IResult> CreateFinanceCashbook(
        [FromBody] CashbookUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var entry = new CashbookEntry
        {
            Id = Guid.NewGuid(),
            BranchId = request.BranchId,
            Type = request.Type,
            Amount = request.Amount,
            Currency = string.IsNullOrWhiteSpace(request.Currency) ? "VND" : request.Currency,
            Description = request.Title ?? request.Note,
            RelatedType = request.Category,
            EntryDate = request.Date ?? DateOnly.FromDateTime(DateTime.UtcNow),
            CreatedBy = _userContext.UserId,
            CreatedAt = DateTime.UtcNow
        };

        _context.CashbookEntries.Add(entry);
        await _context.SaveChangesAsync(cancellationToken);

        return OkData(new { entry.Id });
    }

    [HttpPut("cashbook/{id:guid}")]
    public async Task<IResult> UpdateFinanceCashbook(
        Guid id,
        [FromBody] CashbookUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var entry = await _context.CashbookEntries.FirstOrDefaultAsync(cb => cb.Id == id, cancellationToken);
        if (entry is null)
        {
            return NotFoundProblem("CashbookEntry", "Cashbook entry not found");
        }

        entry.BranchId = request.BranchId;
        entry.Type = request.Type;
        entry.Amount = request.Amount;
        entry.Currency = string.IsNullOrWhiteSpace(request.Currency) ? entry.Currency : request.Currency;
        entry.Description = request.Title ?? request.Note;
        entry.RelatedType = request.Category;
        entry.EntryDate = request.Date ?? entry.EntryDate;

        await _context.SaveChangesAsync(cancellationToken);
        return OkData(new { entry.Id });
    }

    [HttpGet("fees")]
    public async Task<IResult> GetFinanceFees(CancellationToken cancellationToken)
    {
        var rows = await _context.Invoices
            .AsNoTracking()
            .OrderByDescending(i => i.IssuedAt)
            .Take(200)
            .Select(i => new
            {
                studentId = i.StudentProfileId,
                studentName = i.StudentProfile.DisplayName,
                course = i.Class != null ? i.Class.Title : i.Description,
                total = i.Amount,
                paid = i.Payments.Sum(p => (decimal?)p.Amount) ?? 0m,
                remaining = i.Amount - (i.Payments.Sum(p => (decimal?)p.Amount) ?? 0m),
                dueDate = i.DueDate,
                status = i.Status.ToString(),
                progressPercent = i.Amount == 0 ? 0 : Math.Round(((i.Payments.Sum(p => (decimal?)p.Amount) ?? 0m) * 100) / i.Amount, 2)
            })
            .ToListAsync(cancellationToken);

        return OkData(rows);
    }

    [HttpGet("payroll")]
    public async Task<IResult> GetFinancePayroll(
        [FromQuery] string? month,
        [FromQuery] string? department,
        CancellationToken cancellationToken)
    {
        var payrollLines = _context.PayrollLines.AsNoTracking().AsQueryable();
        var payrollPayments = _context.PayrollPayments.AsNoTracking().AsQueryable();
        var workHours = _context.MonthlyWorkHours.AsNoTracking().AsQueryable();

        int? filterYear = null;
        int? filterMonth = null;
        if (!string.IsNullOrWhiteSpace(month))
        {
            var parts = month.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length == 2 && int.TryParse(parts[0], out var parsedYear) && int.TryParse(parts[1], out var parsedMonth))
            {
                filterYear = parsedYear;
                filterMonth = parsedMonth;
            }
        }

        if (filterYear.HasValue && filterMonth.HasValue)
        {
            payrollLines = payrollLines.Where(pl => pl.PayrollRun.PeriodStart.Year == filterYear.Value && pl.PayrollRun.PeriodStart.Month == filterMonth.Value);
            payrollPayments = payrollPayments.Where(pp => pp.PayrollRun.PeriodStart.Year == filterYear.Value && pp.PayrollRun.PeriodStart.Month == filterMonth.Value);
            workHours = workHours.Where(m => m.Year == filterYear.Value && m.Month == filterMonth.Value);
        }

        var rows = await payrollLines
            .GroupBy(pl => new { pl.StaffUserId, employeeName = pl.StaffUser.Name, department = pl.StaffUser.Role.ToString() })
            .Select(g => new
            {
                payrollId = g.Select(x => x.PayrollRunId).FirstOrDefault(),
                employeeId = g.Key.StaffUserId,
                employeeName = g.Key.employeeName,
                department = g.Key.department,
                baseSalary = g.Where(x => x.ComponentType == PayrollComponentType.Base).Sum(x => (decimal?)x.Amount) ?? 0m,
                allowance = g.Where(x => x.ComponentType == PayrollComponentType.Allowance).Sum(x => (decimal?)x.Amount) ?? 0m,
                bonus = g.Where(x =>
                        x.ComponentType == PayrollComponentType.Overtime ||
                        x.ComponentType == PayrollComponentType.Teaching ||
                        x.ComponentType == PayrollComponentType.Ta ||
                        x.ComponentType == PayrollComponentType.Club ||
                        x.ComponentType == PayrollComponentType.Workshop)
                    .Sum(x => (decimal?)x.Amount) ?? 0m,
                deduction = g.Where(x => x.ComponentType == PayrollComponentType.Deduction).Sum(x => (decimal?)x.Amount) ?? 0m,
                total = g.Sum(x => (decimal?)x.Amount) ?? 0m,
                status = g.Select(x => x.PayrollRun.Status.ToString()).FirstOrDefault() ?? "Pending",
                performance = workHours
                    .Where(m => m.StaffUserId == g.Key.StaffUserId)
                    .Select(m => (decimal?)m.TotalHours)
                    .FirstOrDefault() ?? 0m,
                paidAmount = payrollPayments
                    .Where(pp => pp.StaffUserId == g.Key.StaffUserId)
                    .Sum(pp => (decimal?)pp.Amount) ?? 0m
            })
            .ToListAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(department))
        {
            rows = rows
                .Where(x => string.Equals(x.department, department, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return OkData(rows);
    }

    [HttpGet("dues")]
    public async Task<IResult> GetFinanceDues(CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var rawDebts = await _context.Invoices
            .AsNoTracking()
            .Where(i => i.Status == InvoiceStatus.Pending || i.Status == InvoiceStatus.Overdue)
            .OrderBy(i => i.DueDate)
            .Select(i => new
            {
                id = i.Id,
                studentId = i.StudentProfileId,
                studentName = i.StudentProfile.DisplayName,
                className = i.Class != null ? i.Class.Title : null,
                amount = i.Amount - (i.Payments.Sum(p => (decimal?)p.Amount) ?? 0m),
                dueDate = i.DueDate,
                status = i.Status.ToString(),
                lastContact = i.Payments
                    .Where(p => p.PaidAt != null)
                    .OrderByDescending(p => p.PaidAt)
                    .Select(p => p.PaidAt)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        var debts = rawDebts
            .Select(d =>
            {
                var daysOverdue = d.dueDate.HasValue ? Math.Max(0, today.DayNumber - d.dueDate.Value.DayNumber) : 0;
                var bucket = !d.dueDate.HasValue
                    ? "Unknown"
                    : daysOverdue switch
                    {
                        <= 0 => "Current",
                        <= 30 => "1-30",
                        <= 60 => "31-60",
                        <= 90 => "61-90",
                        _ => "90+"
                    };

                return new
                {
                    d.id,
                    d.studentId,
                    d.studentName,
                    d.className,
                    d.amount,
                    d.dueDate,
                    daysOverdue,
                    d.status,
                    bucket,
                    d.lastContact
                };
            })
            .ToList();

        var agingBuckets = debts
            .GroupBy(d => d.bucket)
            .Select(g => new
            {
                bucket = g.Key,
                count = g.Count(),
                amount = g.Sum(x => x.amount)
            })
            .ToList();

        return OkData(new
        {
            agingBuckets,
            debts
        });
    }

    [HttpGet("adjustments")]
    public async Task<IResult> GetFinanceAdjustments(CancellationToken cancellationToken)
    {
        var rawItems = await _context.CashbookEntries
            .AsNoTracking()
            .Where(cb => cb.RelatedType == RelatedType.Adjustment)
            .OrderByDescending(cb => cb.CreatedAt)
            .Take(200)
            .Select(cb => new
            {
                id = cb.Id,
                cb.EntryDate,
                amount = cb.Type == CashbookEntryType.CashOut ? -cb.Amount : cb.Amount,
                type = "Adjustment",
                status = "Recorded",
                timestamp = cb.CreatedAt,
                note = cb.Description,
                user = cb.CreatedByUser != null ? cb.CreatedByUser.Name : null
            })
            .ToListAsync(cancellationToken);

        var items = rawItems
            .Select(cb => new
            {
                cb.id,
                code = $"ADJ-{cb.EntryDate:yyyyMMdd}-{cb.id.ToString().Substring(0, 8)}",
                cb.amount,
                cb.type,
                cb.status,
                cb.timestamp,
                cb.note,
                cb.user
            })
            .ToList();

        return OkData(items);
    }

    [HttpPost("adjustments")]
    public async Task<IResult> CreateFinanceAdjustment(
        [FromBody] AdjustmentCreateRequest request,
        CancellationToken cancellationToken)
    {
        var isCashOut = request.Amount < 0;
        var entry = new CashbookEntry
        {
            Id = Guid.NewGuid(),
            BranchId = request.BranchId,
            Type = isCashOut ? CashbookEntryType.CashOut : CashbookEntryType.CashIn,
            Amount = Math.Abs(request.Amount),
            Currency = string.IsNullOrWhiteSpace(request.Currency) ? "VND" : request.Currency,
            Description = request.Note,
            RelatedType = RelatedType.Adjustment,
            EntryDate = DateOnly.FromDateTime((request.Timestamp ?? DateTime.UtcNow).Date),
            CreatedBy = _userContext.UserId,
            CreatedAt = request.Timestamp ?? DateTime.UtcNow
        };

        _context.CashbookEntries.Add(entry);
        await _context.SaveChangesAsync(cancellationToken);

        return OkData(new
        {
            id = entry.Id,
            code = $"ADJ-{entry.EntryDate:yyyyMMdd}-{entry.Id.ToString()[..8]}",
            amount = isCashOut ? -entry.Amount : entry.Amount,
            type = "Adjustment",
            status = "Recorded",
            timestamp = entry.CreatedAt,
            note = entry.Description,
            user = _userContext.UserId
        });
    }

    [HttpGet("reports")]
    public async Task<IResult> GetFinanceReports(
        [FromQuery] string period = "monthly",
        CancellationToken cancellationToken = default)
    {
        var payments = await _context.Payments
            .AsNoTracking()
            .Where(p => p.PaidAt != null)
            .Select(p => new
            {
                paidAt = p.PaidAt!.Value,
                p.Amount,
                method = p.Method.ToString()
            })
            .ToListAsync(cancellationToken);

        var rows = payments
            .GroupBy(p => GetPeriodKey(period, p.paidAt))
            .Select(g => new
            {
                period = g.Key,
                totalAmount = g.Sum(x => x.Amount),
                transactionCount = g.Count()
            })
            .OrderBy(x => x.period)
            .ToList();

        var paymentMethodBreakdown = payments
            .GroupBy(p => p.method)
            .Select(g => new
            {
                method = g.Key,
                amount = g.Sum(x => x.Amount),
                count = g.Count()
            })
            .ToList();

        var growthTrend = rows
            .Select((row, index) => new
            {
                row.period,
                row.totalAmount,
                growthPercent = index == 0 || rows[index - 1].totalAmount == 0
                    ? 0
                    : Math.Round((row.totalAmount - rows[index - 1].totalAmount) * 100 / rows[index - 1].totalAmount, 2)
            })
            .ToList();

        return OkData(new
        {
            rows,
            summary = new
            {
                totalAmount = payments.Sum(x => x.Amount),
                transactionCount = payments.Count
            },
            paymentMethodBreakdown,
            growthTrend
        });
    }

    [HttpGet("payos/transactions")]
    public async Task<IResult> GetFinancePayOsTransactions(CancellationToken cancellationToken)
    {
        var items = await _context.Payments
            .AsNoTracking()
            .Where(p => p.Method == PaymentMethod.Payos)
            .OrderByDescending(p => p.PaidAt)
            .Take(200)
            .Select(p => new
            {
                transactionId = p.Id,
                studentId = p.Invoice.StudentProfileId,
                studentName = p.Invoice.StudentProfile.DisplayName,
                amount = p.Amount,
                status = p.PaidAt != null ? "Paid" : "Pending",
                method = p.Method.ToString(),
                timestamp = p.PaidAt,
                description = p.Invoice.Description
            })
            .ToListAsync(cancellationToken);

        return OkData(items);
    }

    [HttpPost("payos/generate-link")]
    public async Task<IResult> GenerateFinancePayOsLink(
        [FromBody] InvoiceActionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreatePayOSLinkCommand { InvoiceId = request.InvoiceId }, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("payos/generate-qr")]
    public async Task<IResult> GenerateFinancePayOsQr(
        [FromBody] InvoiceActionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreatePayOSLinkCommand { InvoiceId = request.InvoiceId }, cancellationToken);
        return result.MatchOk();
    }

    private static string GetPeriodKey(string period, DateTime value)
    {
        return period.ToLowerInvariant() switch
        {
            "yearly" => $"{value.Year:D4}",
            "quarterly" => $"{value.Year:D4}-Q{((value.Month - 1) / 3) + 1}",
            _ => $"{value.Year:D4}-{value.Month:D2}"
        };
    }

    private static IResult OkData<T>(T data)
    {
        return Results.Ok(ApiResult<T>.Success(data));
    }

    private static IResult NotFoundProblem(string title, string detail)
    {
        return Results.Problem(title: title, detail: detail, statusCode: StatusCodes.Status404NotFound);
    }
}
