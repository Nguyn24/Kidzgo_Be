using FluentValidation;

namespace Kidzgo.Application.Invoices.UpdateInvoice;

public sealed class UpdateInvoiceCommandValidator : AbstractValidator<UpdateInvoiceCommand>
{
    public UpdateInvoiceCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.Currency)
            .MaximumLength(10).WithMessage("Currency must not exceed 10 characters.")
            .When(x => !string.IsNullOrEmpty(x.Currency));

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0.")
            .When(x => x.Amount.HasValue);
    }
}

