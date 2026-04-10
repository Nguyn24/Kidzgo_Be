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

        var persistedAttempts = await context.HomeworkSubmissionAttempts
            .Where(a => a.HomeworkStudentId == homeworkStudent.Id)
            .OrderByDescending(a => a.AttemptNumber)
            .ToListAsync(cancellationToken);

        var attemptDtos = HomeworkSubmissionAttemptMapper.BuildDtos(homeworkStudent, persistedAttempts);
        var latestAttempt = HomeworkSubmissionAttemptMapper.BuildSnapshots(homeworkStudent, persistedAttempts)
            .FirstOrDefault();

        var effectiveStatus = latestAttempt?.Status ?? homeworkStudent.Status;
        var effectiveSubmittedAt = latestAttempt?.SubmittedAt ?? homeworkStudent.SubmittedAt;
        var effectiveGradedAt = latestAttempt?.GradedAt ?? homeworkStudent.GradedAt;
        var effectiveScore = latestAttempt?.Score ?? homeworkStudent.Score;
        var effectiveTeacherFeedback = latestAttempt?.TeacherFeedback ?? homeworkStudent.TeacherFeedback;
        var effectiveAiFeedback = latestAttempt?.AiFeedback ?? homeworkStudent.AiFeedback;
        var effectiveTextAnswer = latestAttempt?.TextAnswer ?? homeworkStudent.TextAnswer;
        var effectiveAttachmentUrl = latestAttempt?.AttachmentUrl ?? homeworkStudent.AttachmentUrl;

        var now = VietnamTime.UtcNow();
        var isOverdue = homeworkStudent.Assignment.DueAt.HasValue &&
                        now > homeworkStudent.Assignment.DueAt.Value &&
                        (effectiveStatus == HomeworkStatus.Assigned || effectiveStatus == HomeworkStatus.Missing);
        var showReview = effectiveStatus == HomeworkStatus.Graded &&
                         homeworkStudent.Assignment.SubmissionType == SubmissionType.Quiz;

        List<HomeworkQuestionDto> questions = new();
        List<QuizAnswerResultDto> reviewResults = new();

        if (homeworkStudent.Assignment.SubmissionType == SubmissionType.Quiz)
        {
            var reviewData = await QuizSubmissionReviewBuilder.BuildAsync(
                context,
                homeworkStudent.AssignmentId,
                effectiveTextAnswer,
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
            AllowResubmit = homeworkStudent.Assignment.MaxAttempts > 1,
            MaxAttempts = homeworkStudent.Assignment.MaxAttempts,
            AiHintEnabled = homeworkStudent.Assignment.AiHintEnabled,
            AiRecommendEnabled = homeworkStudent.Assignment.AiRecommendEnabled,
            SpeakingMode = homeworkStudent.Assignment.SpeakingMode,
            TargetWords = StringListJson.Deserialize(homeworkStudent.Assignment.TargetWords),
            SpeakingExpectedText = homeworkStudent.Assignment.SpeakingExpectedText,
            Status = effectiveStatus.ToString(),
            SubmittedAt = effectiveSubmittedAt,
            GradedAt = effectiveGradedAt,
            Score = effectiveScore,
            TeacherFeedback = effectiveTeacherFeedback,
            AiFeedback = effectiveAiFeedback,
            AttachmentUrls = effectiveAttachmentUrl,
            TextAnswer = effectiveTextAnswer,
            LinkUrl = homeworkStudent.Assignment.SubmissionType == SubmissionType.Link
                ? effectiveAttachmentUrl
                : null,
            IsLate = effectiveStatus == HomeworkStatus.Late,
            IsOverdue = isOverdue,
            Questions = questions,
            Review = showReview ? new HomeworkReviewDto { AnswerResults = reviewResults } : null,
            AttemptCount = attemptDtos.Count,
            Attempts = attemptDtos,
            StudentProfileId = homeworkStudent.StudentProfileId,
            StudentName = homeworkStudent.StudentProfile.DisplayName
        };
    }
}
