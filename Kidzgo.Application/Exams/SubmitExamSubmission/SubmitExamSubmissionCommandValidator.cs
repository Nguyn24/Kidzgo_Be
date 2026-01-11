using FluentValidation;

namespace Kidzgo.Application.Exams.SubmitExamSubmission;

public sealed class SubmitExamSubmissionCommandValidator : AbstractValidator<SubmitExamSubmissionCommand>
{
    public SubmitExamSubmissionCommandValidator()
    {
        RuleFor(x => x.SubmissionId)
            .NotEmpty()
            .WithMessage("Submission ID is required");
    }
}


