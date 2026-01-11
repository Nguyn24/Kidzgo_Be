using FluentValidation;

namespace Kidzgo.Application.Exams.CreateExam;

public sealed class CreateExamCommandValidator : AbstractValidator<CreateExamCommand>
{
    public CreateExamCommandValidator()
    {
        RuleFor(command => command.ClassId)
            .NotEmpty()
            .WithMessage("Class ID is required");

        RuleFor(command => command.Date)
            .NotEmpty()
            .WithMessage("Date is required");
    }
}

