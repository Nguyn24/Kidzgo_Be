using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Finance.Errors;

public static class InvoiceErrors
{
    public static readonly Error BranchNotFound = Error.NotFound(
        "Invoice.BranchNotFound",
        "Branch not found.");

    public static readonly Error StudentProfileNotFound = Error.NotFound(
        "Invoice.StudentProfileNotFound",
        "Student profile not found.");

    public static readonly Error ClassNotFound = Error.NotFound(
        "Invoice.ClassNotFound",
        "Class not found.");

    public static readonly Error InvoiceNotFound = Error.NotFound(
        "Invoice.InvoiceNotFound",
        "Invoice not found.");

    public static readonly Error InvoiceAlreadyCancelled = Error.Conflict(
        "Invoice.InvoiceAlreadyCancelled",
        "Invoice is already cancelled.");

    public static readonly Error InvoiceAlreadyPaid = Error.Conflict(
        "Invoice.InvoiceAlreadyPaid",
        "Invoice is already paid.");

    public static readonly Error InvoiceLineNotFound = Error.NotFound(
        "Invoice.InvoiceLineNotFound",
        "Invoice line not found.");

    public static readonly Error CannotCancelPaidInvoice = Error.Conflict(
        "Invoice.CannotCancelPaidInvoice",
        "Cannot cancel a paid invoice.");

    public static readonly Error ParentNotFound = Error.NotFound(
        "Invoice.ParentNotFound",
        "Parent profile not found.");
}

