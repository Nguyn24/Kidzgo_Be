using System.Text.Json;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Homework.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Homework.GetStudentHomeworkSubmission;

public sealed class GetStudentHomeworkSubmissionQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetStudentHomeworkSubmissionQuery, GetStudentHomeworkSubmissionResponse>
{
    public async Task<Result<GetStudentHomeworkSubmissionResponse>> Handle(
        GetStudentHomeworkSubmissionQuery query,
        CancellationToken cancellationToken)
    {
        var studentId = userContext.StudentId;

        if (!studentId.HasValue)
        {
            return Result.Failure<GetStudentHomeworkSubmissionResponse>(ProfileErrors.StudentNotFound);
        }

        var homeworkStudent = await context.HomeworkStudents
            .Include(hs => hs.Assignment)
                .ThenInclude(a => a.Class)
            .FirstOrDefaultAsync(hs => hs.Id == query.HomeworkStudentId, cancellationToken);

        if (homeworkStudent is null)
        {
            return Result.Failure<GetStudentHomeworkSubmissionResponse>(
                HomeworkErrors.SubmissionNotFound(query.HomeworkStudentId));
        }

        if (homeworkStudent.StudentProfileId != studentId.Value)
        {
            return Result.Failure<GetStudentHomeworkSubmissionResponse>(
                HomeworkErrors.SubmissionUnauthorized);
        }
        
        var now = DateTime.UtcNow;
        var isOverdue = homeworkStudent.Assignment.DueAt.HasValue && 
                       now > homeworkStudent.Assignment.DueAt.Value && 
                       (homeworkStudent.Status == HomeworkStatus.Assigned || homeworkStudent.Status == HomeworkStatus.Missing);

        List<StudentHomeworkQuestionDto> questions = new();
        var reviewResults = new List<QuizAnswerResultDto>();
        var showReview = homeworkStudent.Status == HomeworkStatus.Graded &&
                         homeworkStudent.Assignment.SubmissionType == SubmissionType.Quiz;
        if (homeworkStudent.Assignment.SubmissionType == SubmissionType.Quiz)
        {
            if (!homeworkStudent.StartedAt.HasValue && homeworkStudent.Status == HomeworkStatus.Assigned)
            {
                homeworkStudent.StartedAt = now;
                await context.SaveChangesAsync(cancellationToken);
            }

            var homeworkQuestions = await context.HomeworkQuestions
                .Where(q => q.HomeworkAssignmentId == homeworkStudent.AssignmentId)
                .OrderBy(q => q.OrderIndex)
                .ToListAsync(cancellationToken);

            var selectedOptionByQuestionId = ParseSelectedOptions(homeworkStudent.TextAnswer);

            foreach (var question in homeworkQuestions)
            {
                var optionTexts = QuizOptionUtils.ParseOptions(question.Options);
                var optionDtos = optionTexts
                    .Select((text, idx) => new StudentHomeworkOptionDto
                    {
                        Id = QuizOptionUtils.BuildOptionId(question.Id, idx),
                        Text = text,
                        OrderIndex = idx
                    })
                    .ToList();
                var optionTextById = optionDtos.ToDictionary(o => o.Id, o => o.Text);

                questions.Add(new StudentHomeworkQuestionDto
                {
                    Id = question.Id,
                    OrderIndex = question.OrderIndex,
                    QuestionText = question.QuestionText,
                    QuestionType = question.QuestionType.ToString(),
                    Options = optionDtos,
                    Points = question.Points
                });

                if (!showReview)
                {
                    continue;
                }

                var correctOptionId = (Guid?)null;
                var correctOptionText = (string?)null;
                if (int.TryParse(question.CorrectAnswer, out var correctIdx) &&
                    correctIdx >= 0 &&
                    correctIdx < optionTexts.Count)
                {
                    correctOptionId = QuizOptionUtils.BuildOptionId(question.Id, correctIdx);
                    correctOptionText = optionTexts[correctIdx];
                }

                var selectedOptionId = selectedOptionByQuestionId.TryGetValue(question.Id, out var selected)
                    ? selected
                    : null;

                var selectedOptionText = selectedOptionId.HasValue
                    ? optionTextById.TryGetValue(selectedOptionId.Value, out var text)
                        ? text
                        : null
                    : null;

                var isCorrect = selectedOptionId.HasValue &&
                                correctOptionId.HasValue &&
                                selectedOptionId == correctOptionId;

                reviewResults.Add(new QuizAnswerResultDto
                {
                    QuestionId = question.Id,
                    QuestionText = question.QuestionText,
                    SelectedOptionId = selectedOptionId,
                    SelectedOptionText = selectedOptionText,
                    CorrectOptionId = correctOptionId,
                    CorrectOptionText = correctOptionText,
                    IsCorrect = isCorrect,
                    EarnedPoints = isCorrect ? question.Points : 0,
                    MaxPoints = question.Points,
                    Explanation = question.Explanation
                });
            }
        }

        return new GetStudentHomeworkSubmissionResponse
        {
            Id = homeworkStudent.Id,
            HomeworkStudentId = homeworkStudent.Id,
            AssignmentId = homeworkStudent.AssignmentId,
            AssignmentTitle = homeworkStudent.Assignment.Title,
            AssignmentDescription = homeworkStudent.Assignment.Description,
            Instructions = homeworkStudent.Assignment.Instructions,
            ClassId = homeworkStudent.Assignment.ClassId,
            ClassCode = homeworkStudent.Assignment.Class.Code,
            ClassTitle = homeworkStudent.Assignment.Class.Title,
            DueAt = homeworkStudent.Assignment.DueAt,
            Book = homeworkStudent.Assignment.Book,
            Pages = homeworkStudent.Assignment.Pages,
            Skills = homeworkStudent.Assignment.Skills,
            SubmissionType = SubmissionTypeMapper.ToApiString(homeworkStudent.Assignment.SubmissionType),
            MaxScore = homeworkStudent.Assignment.MaxScore,
            RewardStars = homeworkStudent.Assignment.RewardStars,
            TimeLimitMinutes = homeworkStudent.Assignment.TimeLimitMinutes,
            AllowResubmit = homeworkStudent.Assignment.AllowResubmit,
            Status = homeworkStudent.Status.ToString(),
            StartedAt = homeworkStudent.StartedAt,
            SubmittedAt = homeworkStudent.SubmittedAt,
            GradedAt = homeworkStudent.GradedAt,
            Score = homeworkStudent.Score,
            TeacherFeedback = homeworkStudent.TeacherFeedback,
            AiFeedback = homeworkStudent.AiFeedback,
            AttachmentUrls = homeworkStudent.AttachmentUrl,
            TextAnswer = homeworkStudent.TextAnswer,
            IsLate = homeworkStudent.Status == HomeworkStatus.Late,
            IsOverdue = isOverdue,
            Questions = questions,
            Review = showReview ? new StudentHomeworkReviewDto { AnswerResults = reviewResults } : null,
            ShowReview = showReview,
            ShowCorrectAnswer = showReview,
            ShowExplanation = showReview
        };
    }

    private static Dictionary<Guid, Guid?> ParseSelectedOptions(string? textAnswer)
    {
        var result = new Dictionary<Guid, Guid?>();
        if (string.IsNullOrWhiteSpace(textAnswer))
        {
            return result;
        }

        try
        {
            using var doc = JsonDocument.Parse(textAnswer);
            if (doc.RootElement.ValueKind != JsonValueKind.Array)
            {
                return result;
            }

            foreach (var item in doc.RootElement.EnumerateArray())
            {
                if (!item.TryGetProperty("questionId", out var questionIdProp) ||
                    questionIdProp.ValueKind != JsonValueKind.String ||
                    !Guid.TryParse(questionIdProp.GetString(), out var questionId))
                {
                    continue;
                }

                if (item.TryGetProperty("selectedOptionId", out var selectedProp))
                {
                    if (selectedProp.ValueKind == JsonValueKind.String &&
                        Guid.TryParse(selectedProp.GetString(), out var selectedId))
                    {
                        result[questionId] = selectedId;
                    }
                    else
                    {
                        result[questionId] = null;
                    }
                }
                else if (item.TryGetProperty("answer", out var answerProp))
                {
                    if (answerProp.ValueKind == JsonValueKind.String &&
                        Guid.TryParse(answerProp.GetString(), out var legacySelectedId))
                    {
                        result[questionId] = legacySelectedId;
                    }
                }
            }
        }
        catch
        {
            return result;
        }

        return result;
    }
}
