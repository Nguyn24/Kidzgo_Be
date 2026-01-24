using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Exams;
using Kidzgo.Domain.Exams.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Exams.DeleteExamQuestion;

public sealed class DeleteExamQuestionCommandHandler(
    IDbContext context
) : ICommandHandler<DeleteExamQuestionCommand, DeleteExamQuestionResponse>
{
    public async Task<Result<DeleteExamQuestionResponse>> Handle(
        DeleteExamQuestionCommand command,
        CancellationToken cancellationToken)
    {
        var question = await context.ExamQuestions
            .FirstOrDefaultAsync(q => q.Id == command.QuestionId, cancellationToken);

        if (question is null)
        {
            return Result.Failure<DeleteExamQuestionResponse>(
                ExamQuestionErrors.NotFound(command.QuestionId));
        }

        // Check if there are any submissions with answers for this question
        var hasSubmissions = await context.ExamSubmissionAnswers
            .AnyAsync(a => a.QuestionId == command.QuestionId, cancellationToken);

        if (hasSubmissions)
        {
            return Result.Failure<DeleteExamQuestionResponse>(ExamQuestionErrors.HasSubmissions);
        }

        context.ExamQuestions.Remove(question);
        await context.SaveChangesAsync(cancellationToken);

        return new DeleteExamQuestionResponse
        {
            Id = question.Id
        };
    }
}


