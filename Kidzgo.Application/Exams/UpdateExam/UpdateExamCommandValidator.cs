using FluentValidation;

namespace Kidzgo.Application.Exams.UpdateExam;

public sealed class UpdateExamCommandValidator : AbstractValidator<UpdateExamCommand>
{
    public UpdateExamCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty()
            .WithMessage("Exam ID is required");
    }
}

