using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.Homework.Errors;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Homework.GradeHomework;

public sealed class GradeHomeworkCommandHandler(
    IDbContext context
) : ICommandHandler<GradeHomeworkCommand, GradeHomeworkResponse>
{
    public async Task<Result<GradeHomeworkResponse>> Handle(
        GradeHomeworkCommand command,
        CancellationToken cancellationToken)
    {
        var homeworkStudent = await context.HomeworkStudents
            .Include(hs => hs.Assignment)
            .FirstOrDefaultAsync(hs => hs.Id == command.HomeworkStudentId, cancellationToken);

        if (homeworkStudent is null)
        {
            return Result.Failure<GradeHomeworkResponse>(
                HomeworkErrors.SubmissionNotFound(command.HomeworkStudentId));
        }

        // Validate score is within range
        if (command.Score < 0)
        {
            return Result.Failure<GradeHomeworkResponse>(
                HomeworkErrors.SubmissionInvalidScore);
        }

        if (homeworkStudent.Assignment.MaxScore.HasValue && command.Score > homeworkStudent.Assignment.MaxScore.Value)
        {
            return Result.Failure<GradeHomeworkResponse>(
                HomeworkErrors.SubmissionScoreExceedsMax(homeworkStudent.Assignment.MaxScore.Value));
        }

        // Can only grade if status is Submitted or Graded (allow re-grading)
        if (homeworkStudent.Status != HomeworkStatus.Submitted && homeworkStudent.Status != HomeworkStatus.Graded)
        {
            return Result.Failure<GradeHomeworkResponse>(
                HomeworkErrors.SubmissionNotSubmitted);
        }

        // Update status, score, and feedback
        homeworkStudent.Status = HomeworkStatus.Graded;
        homeworkStudent.Score = command.Score;
        homeworkStudent.TeacherFeedback = command.TeacherFeedback;
        homeworkStudent.GradedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return new GradeHomeworkResponse
        {
            Id = homeworkStudent.Id,
            AssignmentId = homeworkStudent.AssignmentId,
            Status = homeworkStudent.Status.ToString(),
            Score = homeworkStudent.Score!.Value,
            TeacherFeedback = homeworkStudent.TeacherFeedback,
            GradedAt = homeworkStudent.GradedAt!.Value
        };
    }
}

