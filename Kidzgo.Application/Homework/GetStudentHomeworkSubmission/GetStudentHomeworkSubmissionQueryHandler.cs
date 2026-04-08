using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Homework.Shared;
using Kidzgo.Application.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.Homework.Errors;
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
            .Include(hs => hs.Assignment)
                .ThenInclude(a => a.CreatedByUser)
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

        var persistedAttempts = await context.HomeworkSubmissionAttempts
            .Where(a => a.HomeworkStudentId == homeworkStudent.Id)
            .OrderByDescending(a => a.AttemptNumber)
            .ToListAsync(cancellationToken);

        var attemptSnapshots = HomeworkSubmissionAttemptMapper.BuildSnapshots(homeworkStudent, persistedAttempts);
        var attemptDtos = HomeworkSubmissionAttemptMapper.BuildDtos(homeworkStudent, persistedAttempts);

        HomeworkSubmissionAttempt? selectedAttempt = null;
        if (query.AttemptNumber.HasValue)
        {
            selectedAttempt = attemptSnapshots.FirstOrDefault(a => a.AttemptNumber == query.AttemptNumber.Value);
            if (selectedAttempt is null)
            {
                return Result.Failure<GetStudentHomeworkSubmissionResponse>(
                    HomeworkErrors.SubmissionAttemptNotFound(query.HomeworkStudentId, query.AttemptNumber.Value));
            }
        }
        else
        {
            selectedAttempt = attemptSnapshots.FirstOrDefault();
        }

        var effectiveStatus = selectedAttempt?.Status ?? homeworkStudent.Status;
        var effectiveStartedAt = selectedAttempt?.StartedAt ?? homeworkStudent.StartedAt;
        var effectiveSubmittedAt = selectedAttempt?.SubmittedAt ?? homeworkStudent.SubmittedAt;
        var effectiveGradedAt = selectedAttempt?.GradedAt ?? homeworkStudent.GradedAt;
        var effectiveScore = selectedAttempt?.Score ?? homeworkStudent.Score;
        var effectiveTeacherFeedback = selectedAttempt?.TeacherFeedback ?? homeworkStudent.TeacherFeedback;
        var effectiveAiFeedback = selectedAttempt?.AiFeedback ?? homeworkStudent.AiFeedback;
        var effectiveTextAnswer = selectedAttempt?.TextAnswer ?? homeworkStudent.TextAnswer;
        var effectiveAttachmentUrl = selectedAttempt?.AttachmentUrl ?? homeworkStudent.AttachmentUrl;

        var now = VietnamTime.UtcNow();
        var isOverdue = homeworkStudent.Assignment.DueAt.HasValue &&
                        now > homeworkStudent.Assignment.DueAt.Value &&
                        (effectiveStatus == HomeworkStatus.Assigned || effectiveStatus == HomeworkStatus.Missing);

        List<HomeworkQuestionDto> questions = new();
        var reviewResults = new List<QuizAnswerResultDto>();
        var showReview = effectiveStatus == HomeworkStatus.Graded &&
                         homeworkStudent.Assignment.SubmissionType == SubmissionType.Quiz;

        if (homeworkStudent.Assignment.SubmissionType == SubmissionType.Quiz)
        {
            if (!query.AttemptNumber.HasValue &&
                !homeworkStudent.StartedAt.HasValue &&
                homeworkStudent.Status == HomeworkStatus.Assigned)
            {
                homeworkStudent.StartedAt = now;
                effectiveStartedAt = now;
                await context.SaveChangesAsync(cancellationToken);
            }

            var reviewData = await QuizSubmissionReviewBuilder.BuildAsync(
                context,
                homeworkStudent.AssignmentId,
                effectiveTextAnswer,
                showReview,
                cancellationToken);

            questions = reviewData.Questions;
            reviewResults = reviewData.AnswerResults;
        }

        return new GetStudentHomeworkSubmissionResponse
        {
            Id = homeworkStudent.Id,
            HomeworkStudentId = homeworkStudent.Id,
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
            RewardStars = homeworkStudent.Assignment.RewardStars,
            TimeLimitMinutes = homeworkStudent.Assignment.TimeLimitMinutes,
            AllowResubmit = homeworkStudent.Assignment.MaxAttempts > 1,
            MaxAttempts = homeworkStudent.Assignment.MaxAttempts,
            AiHintEnabled = homeworkStudent.Assignment.AiHintEnabled,
            AiRecommendEnabled = homeworkStudent.Assignment.AiRecommendEnabled,
            SpeakingMode = homeworkStudent.Assignment.SpeakingMode,
            TargetWords = StringListJson.Deserialize(homeworkStudent.Assignment.TargetWords),
            SpeakingExpectedText = homeworkStudent.Assignment.SpeakingExpectedText,
            Status = effectiveStatus.ToString(),
            StartedAt = effectiveStartedAt,
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
            ShowReview = showReview,
            ShowCorrectAnswer = showReview,
            ShowExplanation = showReview,
            AttemptId = selectedAttempt?.Id == Guid.Empty ? null : selectedAttempt?.Id,
            AttemptNumber = selectedAttempt?.AttemptNumber,
            AttemptCount = attemptDtos.Count,
            Attempts = attemptDtos,
            TeacherName = homeworkStudent.Assignment.CreatedByUser != null
                ? homeworkStudent.Assignment.CreatedByUser.Name
                : null,
            AssignmentAttachmentUrls = string.IsNullOrWhiteSpace(homeworkStudent.Assignment.AttachmentUrl)
                ? new List<string>()
                : new List<string> { homeworkStudent.Assignment.AttachmentUrl },
            Submission = new HomeworkSubmissionPayloadDto
            {
                TextAnswer = effectiveTextAnswer,
                AttachmentUrls = homeworkStudent.Assignment.SubmissionType == SubmissionType.Link ||
                                 string.IsNullOrWhiteSpace(effectiveAttachmentUrl)
                    ? new List<string>()
                    : new List<string> { effectiveAttachmentUrl },
                Links = !string.IsNullOrWhiteSpace(effectiveAttachmentUrl) &&
                        homeworkStudent.Assignment.SubmissionType == SubmissionType.Link
                    ? new List<string> { effectiveAttachmentUrl }
                    : new List<string>(),
                SubmittedAt = effectiveSubmittedAt,
                GradedAt = effectiveGradedAt,
                Score = effectiveScore,
                TeacherFeedback = effectiveTeacherFeedback
            }
        };
    }
}
