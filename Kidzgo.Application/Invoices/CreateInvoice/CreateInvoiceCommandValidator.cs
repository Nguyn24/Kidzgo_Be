using FluentValidation;

namespace Kidzgo.Application.Invoices.CreateInvoice;

public sealed class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator()
    {
        RuleFor(x => x.BranchId)
            .NotEmpty().WithMessage("BranchId is required.");

        RuleFor(x => x.StudentProfileId)
            .NotEmpty().WithMessage("StudentProfileId is required.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid InvoiceType.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .MaximumLength(10).WithMessage("Currency must not exceed 10 characters.");

        RuleForEach(x => x.InvoiceLines)
            .SetValidator(new CreateInvoiceLineDtoValidator());
    }
}

public sealed class CreateInvoiceLineDtoValidator : AbstractValidator<CreateInvoiceLineDto>
{
    public CreateInvoiceLineDtoValidator()
    {
        RuleFor(x => x.ItemType)
            .IsInEnum().WithMessage("Invalid InvoiceLineItemType.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("UnitPrice must be greater than or equal to 0.");
    }
}

