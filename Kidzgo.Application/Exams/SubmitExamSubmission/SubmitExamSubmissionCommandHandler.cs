using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Exams;
using Kidzgo.Domain.Exams.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Exams.SubmitExamSubmission;

public sealed class SubmitExamSubmissionCommandHandler(
    IDbContext context
) : ICommandHandler<SubmitExamSubmissionCommand, SubmitExamSubmissionResponse>
{
    public async Task<Result<SubmitExamSubmissionResponse>> Handle(
        SubmitExamSubmissionCommand command,
        CancellationToken cancellationToken)
    {
        var submission = await context.ExamSubmissions
            .Include(s => s.Exam)
            .Include(s => s.SubmissionAnswers)
                .ThenInclude(a => a.Question)
            .FirstOrDefaultAsync(s => s.Id == command.SubmissionId, cancellationToken);

        if (submission is null)
        {
            return Result.Failure<SubmitExamSubmissionResponse>(
                ExamSubmissionErrors.NotFound(command.SubmissionId));
        }

        if (submission.Status != ExamSubmissionStatus.InProgress)
        {
            return Result.Failure<SubmitExamSubmissionResponse>(
                ExamSubmissionErrors.AlreadySubmitted);
        }

        var now = DateTime.UtcNow;

        // Calculate time spent
        if (submission.ActualStartTime.HasValue)
        {
            var timeSpent = (now - submission.ActualStartTime.Value).TotalMinutes;
            submission.TimeSpentMinutes = (int)timeSpent;
        }

        // Calculate auto score (for multiple choice questions)
        decimal? autoScore = null;
        if (submission.SubmissionAnswers.Any())
        {
            var totalPoints = submission.SubmissionAnswers
                .Where(a => a.Question.QuestionType == QuestionType.MultipleChoice)
                .Sum(a => a.PointsAwarded ?? 0);
            
            autoScore = totalPoints;
            submission.AutoScore = autoScore;
        }

        // Update submission
        if (command.IsAutoSubmit)
        {
            submission.AutoSubmittedAt = now;
            submission.Status = ExamSubmissionStatus.AutoSubmitted;
        }
        else
        {
            submission.SubmittedAt = now;
            submission.Status = ExamSubmissionStatus.Submitted;
        }

        await context.SaveChangesAsync(cancellationToken);

        return new SubmitExamSubmissionResponse
        {
            Id = submission.Id,
            ExamId = submission.ExamId,
            StudentProfileId = submission.StudentProfileId,
            SubmittedAt = submission.SubmittedAt,
            AutoSubmittedAt = submission.AutoSubmittedAt,
            Status = submission.Status,
            AutoScore = submission.AutoScore
        };
    }
}


