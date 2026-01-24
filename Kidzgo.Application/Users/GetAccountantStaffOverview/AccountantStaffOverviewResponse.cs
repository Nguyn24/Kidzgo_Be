namespace Kidzgo.Application.Users.GetAccountantStaffOverview;

public sealed class AccountantStaffOverviewResponse
{
    public DashboardStatistics Statistics { get; set; } = new();
    public List<InvoiceSummaryDto> PendingInvoices { get; set; } = new();
    public List<InvoiceSummaryDto> OverdueInvoices { get; set; } = new();
    public List<PaymentSummaryDto> RecentPayments { get; set; } = new();
    public List<DebtSummaryDto> DebtSummary { get; set; } = new();
    public List<PayrollSummaryDto> PendingPayrolls { get; set; } = new();
    public List<CashbookSummaryDto> RecentCashbookEntries { get; set; } = new();
}

public sealed class DashboardStatistics
{
    public decimal TotalRevenue { get; set; }
    public decimal PendingPayments { get; set; }
    public decimal OverdueAmount { get; set; }
    public int PendingInvoices { get; set; }
    public int OverdueInvoices { get; set; }
    public int PendingPayrolls { get; set; }
    public decimal CashBalance { get; set; }
}

public sealed class InvoiceSummaryDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = null!;
    public string StudentName { get; set; } = null!;
    public decimal Amount { get; set; }
    public string PaymentStatus { get; set; } = null!;
    public DateTime? DueDate { get; set; }
    public DateTime? IssuedAt { get; set; }
}

public sealed class PaymentSummaryDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = null!;
    public string StudentName { get; set; } = null!;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public DateTime PaidAt { get; set; }
}

public sealed class DebtSummaryDto
{
    public Guid StudentProfileId { get; set; }
    public string StudentName { get; set; } = null!;
    public decimal TotalDebt { get; set; }
    public int OverdueDays { get; set; }
    public int InvoiceCount { get; set; }
}

public sealed class PayrollSummaryDto
{
    public Guid Id { get; set; }
    public string PayrollPeriod { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public sealed class CashbookSummaryDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = null!; // CashIn or CashOut
    public decimal Amount { get; set; }
    public string Description { get; set; } = null!;
    public DateTime TransactionDate { get; set; }
}

