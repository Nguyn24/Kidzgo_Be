using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Exams;
using Kidzgo.Domain.Exams.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Exams.GradeExamSubmission;

public sealed class GradeExamSubmissionCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<GradeExamSubmissionCommand, GradeExamSubmissionResponse>
{
    public async Task<Result<GradeExamSubmissionResponse>> Handle(
        GradeExamSubmissionCommand command,
        CancellationToken cancellationToken)
    {
        var submission = await context.ExamSubmissions
            .Include(s => s.SubmissionAnswers)
            .FirstOrDefaultAsync(s => s.Id == command.SubmissionId, cancellationToken);

        if (submission is null)
        {
            return Result.Failure<GradeExamSubmissionResponse>(
                ExamSubmissionErrors.NotFound(command.SubmissionId));
        }

        if (submission.Status != ExamSubmissionStatus.Submitted && 
            submission.Status != ExamSubmissionStatus.AutoSubmitted)
        {
            return Result.Failure<GradeExamSubmissionResponse>(
                Error.Validation("ExamSubmission.InvalidStatus", 
                    "Can only grade submitted or auto-submitted exams"));
        }

        if (submission.Status == ExamSubmissionStatus.Graded)
        {
            return Result.Failure<GradeExamSubmissionResponse>(
                ExamSubmissionErrors.AlreadyGraded);
        }

        var now = DateTime.UtcNow;
        var currentUserId = userContext.UserId;

        // Update individual answer grades if provided
        if (command.AnswerGrades != null && command.AnswerGrades.Any())
        {
            foreach (var answerGrade in command.AnswerGrades)
            {
                var answer = submission.SubmissionAnswers
                    .FirstOrDefault(a => a.QuestionId == answerGrade.QuestionId);

                if (answer != null)
                {
                    if (answerGrade.PointsAwarded.HasValue)
                    {
                        answer.PointsAwarded = answerGrade.PointsAwarded.Value;
                    }

                    if (answerGrade.TeacherFeedback != null)
                    {
                        answer.TeacherFeedback = answerGrade.TeacherFeedback;
                    }
                }
            }
        }

        // Update submission
        if (command.FinalScore.HasValue)
        {
            submission.FinalScore = command.FinalScore.Value;
        }
        else if (command.AnswerGrades != null && command.AnswerGrades.Any())
        {
            // Calculate final score from answer grades
            var totalPoints = submission.SubmissionAnswers
                .Sum(a => a.PointsAwarded ?? 0);
            submission.FinalScore = totalPoints;
        }
        else
        {
            // Use auto score if no manual grading
            submission.FinalScore = submission.AutoScore;
        }

        if (command.TeacherComment != null)
        {
            submission.TeacherComment = command.TeacherComment;
        }

        submission.GradedBy = currentUserId;
        submission.GradedAt = now;
        submission.Status = ExamSubmissionStatus.Graded;

        await context.SaveChangesAsync(cancellationToken);

        return new GradeExamSubmissionResponse
        {
            Id = submission.Id,
            ExamId = submission.ExamId,
            StudentProfileId = submission.StudentProfileId,
            FinalScore = submission.FinalScore,
            TeacherComment = submission.TeacherComment,
            GradedBy = submission.GradedBy,
            GradedAt = submission.GradedAt,
            Status = submission.Status
        };
    }
}


