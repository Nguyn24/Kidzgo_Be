using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Exercises.GetExercises;

public sealed class GetExercisesQueryHandler(
    IDbContext context
) : IQueryHandler<GetExercisesQuery, GetExercisesResponse>
{
    public async Task<Result<GetExercisesResponse>> Handle(GetExercisesQuery query, CancellationToken cancellationToken)
    {
        var exercisesQuery = context.Exercises
            .Include(e => e.Questions)
            .Where(e => !e.IsDeleted)
            .AsQueryable();

        if (query.ClassId.HasValue)
        {
            exercisesQuery = exercisesQuery.Where(e => e.ClassId == query.ClassId.Value);
        }

        if (query.MissionId.HasValue)
        {
            exercisesQuery = exercisesQuery.Where(e => e.MissionId == query.MissionId.Value);
        }

        if (query.ExerciseType.HasValue)
        {
            exercisesQuery = exercisesQuery.Where(e => e.ExerciseType == query.ExerciseType.Value);
        }

        if (query.IsActive.HasValue)
        {
            exercisesQuery = exercisesQuery.Where(e => e.IsActive == query.IsActive.Value);
        }

        int totalCount = await exercisesQuery.CountAsync(cancellationToken);

        var exercises = await exercisesQuery
            .OrderByDescending(e => e.UpdatedAt)
            .ThenByDescending(e => e.CreatedAt)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(e => new ExerciseDto
            {
                Id = e.Id,
                ClassId = e.ClassId,
                MissionId = e.MissionId,
                Title = e.Title,
                Description = e.Description,
                ExerciseType = e.ExerciseType.ToString(),
                IsActive = e.IsActive,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
                QuestionCount = e.Questions.Count
            })
            .ToListAsync(cancellationToken);

        var page = new Page<ExerciseDto>(
            exercises,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetExercisesResponse { Exercises = page };
    }
}


