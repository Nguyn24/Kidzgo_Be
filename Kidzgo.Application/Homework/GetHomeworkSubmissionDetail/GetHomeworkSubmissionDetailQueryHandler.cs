using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Application.Homework.Shared;
using Kidzgo.Application.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.Homework.Errors;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Homework.GetHomeworkSubmissionDetail;

public sealed class GetHomeworkSubmissionDetailQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetHomeworkSubmissionDetailQuery, GetHomeworkSubmissionDetailResponse>
{
    public async Task<Result<GetHomeworkSubmissionDetailResponse>> Handle(
        GetHomeworkSubmissionDetailQuery query,
        CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        var homeworkStudent = await context.HomeworkStudents
            .Include(hs => hs.Assignment)
                .ThenInclude(a => a.Class)
            .Include(hs => hs.StudentProfile)
            .FirstOrDefaultAsync(hs => hs.Id == query.HomeworkStudentId, cancellationToken);

        if (homeworkStudent is null)
        {
            return Result.Failure<GetHomeworkSubmissionDetailResponse>(
                HomeworkErrors.SubmissionNotFound(query.HomeworkStudentId));
        }

        var teacherClassIds = await context.Classes
            .Where(c => c.MainTeacherId == userId || c.AssistantTeacherId == userId)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        if (!teacherClassIds.Contains(homeworkStudent.Assignment.ClassId))
        {
            return Result.Failure<GetHomeworkSubmissionDetailResponse>(
                HomeworkErrors.SubmissionUnauthorized);
        }

        var now = DateTime.UtcNow;
        var isOverdue = homeworkStudent.Assignment.DueAt.HasValue && 
                       now > homeworkStudent.Assignment.DueAt.Value && 
                       (homeworkStudent.Status == HomeworkStatus.Assigned || homeworkStudent.Status == HomeworkStatus.Missing);
        var showReview = homeworkStudent.Status == HomeworkStatus.Graded &&
                         homeworkStudent.Assignment.SubmissionType == SubmissionType.Quiz;
        List<HomeworkQuestionDto> questions = new();
        List<QuizAnswerResultDto> reviewResults = new();

        if (homeworkStudent.Assignment.SubmissionType == SubmissionType.Quiz)
        {
            var reviewData = await QuizSubmissionReviewBuilder.BuildAsync(
                context,
                homeworkStudent.AssignmentId,
                homeworkStudent.TextAnswer,
                showReview,
                cancellationToken);

            questions = reviewData.Questions;
            reviewResults = reviewData.AnswerResults;
        }

        return new GetHomeworkSubmissionDetailResponse
        {
            Id = homeworkStudent.Id,
            AssignmentId = homeworkStudent.AssignmentId,
            AssignmentTitle = homeworkStudent.Assignment.Title,
            AssignmentDescription = homeworkStudent.Assignment.Description,
            AssignmentAttachmentUrl = homeworkStudent.Assignment.AttachmentUrl,
            Instructions = homeworkStudent.Assignment.Instructions,
            ClassId = homeworkStudent.Assignment.ClassId,
            ClassCode = homeworkStudent.Assignment.Class.Code,
            ClassTitle = homeworkStudent.Assignment.Class.Title,
            DueAt = homeworkStudent.Assignment.DueAt,
            Book = homeworkStudent.Assignment.Book,
            Pages = homeworkStudent.Assignment.Pages,
            Skills = homeworkStudent.Assignment.Skills,
            Topic = homeworkStudent.Assignment.Topic,
            GrammarTags = StringListJson.Deserialize(homeworkStudent.Assignment.GrammarTags),
            VocabularyTags = StringListJson.Deserialize(homeworkStudent.Assignment.VocabularyTags),
            SubmissionType = SubmissionTypeMapper.ToApiString(homeworkStudent.Assignment.SubmissionType),
            MaxScore = homeworkStudent.Assignment.MaxScore,
            AiHintEnabled = homeworkStudent.Assignment.AiHintEnabled,
            AiRecommendEnabled = homeworkStudent.Assignment.AiRecommendEnabled,
            SpeakingMode = homeworkStudent.Assignment.SpeakingMode,
            TargetWords = StringListJson.Deserialize(homeworkStudent.Assignment.TargetWords),
            SpeakingExpectedText = homeworkStudent.Assignment.SpeakingExpectedText,
            Status = homeworkStudent.Status.ToString(),
            SubmittedAt = homeworkStudent.SubmittedAt,
            GradedAt = homeworkStudent.GradedAt,
            Score = homeworkStudent.Score,
            TeacherFeedback = homeworkStudent.TeacherFeedback,
            AiFeedback = homeworkStudent.AiFeedback,
            AttachmentUrls = homeworkStudent.AttachmentUrl,
            TextAnswer = homeworkStudent.TextAnswer,
            LinkUrl = homeworkStudent.Assignment.SubmissionType == SubmissionType.Link
                ? homeworkStudent.AttachmentUrl
                : null,
            IsLate = homeworkStudent.Status == HomeworkStatus.Late,
            IsOverdue = isOverdue,
            Questions = questions,
            Review = showReview ? new HomeworkReviewDto { AnswerResults = reviewResults } : null,
            StudentProfileId = homeworkStudent.StudentProfileId,
            StudentName = homeworkStudent.StudentProfile.DisplayName
        };
    }
}
