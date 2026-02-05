using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Exams.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Exercises.GetExerciseById;

public sealed class GetExerciseByIdQueryHandler(
    IDbContext context
) : IQueryHandler<GetExerciseByIdQuery, GetExerciseByIdResponse>
{
    public async Task<Result<GetExerciseByIdResponse>> Handle(GetExerciseByIdQuery query, CancellationToken cancellationToken)
    {
        var exercise = await context.Exercises
            .Include(e => e.Questions)
            .FirstOrDefaultAsync(e => e.Id == query.Id && !e.IsDeleted, cancellationToken);

        if (exercise is null)
        {
            return Result.Failure<GetExerciseByIdResponse>(ExerciseErrors.NotFound(query.Id));
        }

        return new GetExerciseByIdResponse
        {
            Id = exercise.Id,
            ClassId = exercise.ClassId,
            MissionId = exercise.MissionId,
            Title = exercise.Title,
            Description = exercise.Description,
            ExerciseType = exercise.ExerciseType.ToString(),
            IsActive = exercise.IsActive,
            CreatedAt = exercise.CreatedAt,
            UpdatedAt = exercise.UpdatedAt,
            Questions = exercise.Questions
                .OrderBy(q => q.OrderIndex)
                .Select(q => new ExerciseQuestionItem
                {
                    Id = q.Id,
                    OrderIndex = q.OrderIndex,
                    QuestionText = q.QuestionText,
                    QuestionType = q.QuestionType.ToString(),
                    Options = q.Options,
                    CorrectAnswer = q.CorrectAnswer,
                    Points = q.Points,
                    Explanation = q.Explanation
                })
                .ToList()
        };
    }
}


