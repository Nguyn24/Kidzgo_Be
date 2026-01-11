using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Exams;
using Kidzgo.Domain.Exams.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Exams.DeleteExam;

public sealed class DeleteExamCommandHandler(
    IDbContext context
) : ICommandHandler<DeleteExamCommand, DeleteExamResponse>
{
    public async Task<Result<DeleteExamResponse>> Handle(DeleteExamCommand command, CancellationToken cancellationToken)
    {
        var exam = await context.Exams
            .FirstOrDefaultAsync(e => e.Id == command.Id, cancellationToken);

        if (exam == null)
        {
            return Result.Failure<DeleteExamResponse>(
                ExamErrors.NotFound(command.Id));
        }

        // Delete exam results first
        var examResults = await context.ExamResults
            .Where(er => er.ExamId == exam.Id)
            .ToListAsync(cancellationToken);

        context.ExamResults.RemoveRange(examResults);

        // Delete exam
        context.Exams.Remove(exam);

        await context.SaveChangesAsync(cancellationToken);

        return new DeleteExamResponse
        {
            Id = exam.Id
        };
    }
}

