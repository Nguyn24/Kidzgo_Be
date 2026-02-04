using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Exams;
using Kidzgo.Domain.Exams.Errors;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Exercises.Student.GetStudentExerciseById;

public sealed class GetStudentExerciseByIdQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetStudentExerciseByIdQuery, GetStudentExerciseByIdResponse>
{
    public async Task<Result<GetStudentExerciseByIdResponse>> Handle(GetStudentExerciseByIdQuery query, CancellationToken cancellationToken)
    {
        var studentId = userContext.StudentId;
        if (!studentId.HasValue)
        {
            return Result.Failure<GetStudentExerciseByIdResponse>(ProfileErrors.StudentNotFound);
        }

        var exercise = await context.Exercises
            .Include(e => e.Questions)
            .FirstOrDefaultAsync(e => e.Id == query.ExerciseId && !e.IsDeleted && e.IsActive, cancellationToken);

        if (exercise is null)
        {
            return Result.Failure<GetStudentExerciseByIdResponse>(ExerciseErrors.NotFound(query.ExerciseId));
        }

        // Get or create the student's submission for this exercise
        var submission = await context.ExerciseSubmissions
            .Include(s => s.SubmissionAnswers)
            .FirstOrDefaultAsync(s => s.ExerciseId == exercise.Id && s.StudentProfileId == studentId.Value, cancellationToken);

        if (submission is null)
        {
            var now = DateTime.UtcNow;
            submission = new ExerciseSubmission
            {
                Id = Guid.NewGuid(),
                ExerciseId = exercise.Id,
                StudentProfileId = studentId.Value,
                Answers = null,
                Score = null,
                SubmittedAt = now, // kept non-null per schema; will be overwritten on submit
                GradedAt = null,
                GradedBy = null,
                CreatedAt = now,
                UpdatedAt = now
            };

            context.ExerciseSubmissions.Add(submission);
            await context.SaveChangesAsync(cancellationToken);
        }

        return new GetStudentExerciseByIdResponse
        {
            Id = exercise.Id,
            SubmissionId = submission.Id,
            Title = exercise.Title,
            Description = exercise.Description,
            ExerciseType = exercise.ExerciseType.ToString(),
            Questions = exercise.Questions
                .OrderBy(q => q.OrderIndex)
                .Select(q => new StudentExerciseQuestionItem
                {
                    Id = q.Id,
                    OrderIndex = q.OrderIndex,
                    QuestionText = q.QuestionText,
                    QuestionType = q.QuestionType.ToString(),
                    Options = q.Options,
                    Points = q.Points,
                    Explanation = q.Explanation,
                    MyAnswer = submission.SubmissionAnswers.FirstOrDefault(a => a.QuestionId == q.Id)?.Answer
                })
                .ToList()
        };
    }
}


