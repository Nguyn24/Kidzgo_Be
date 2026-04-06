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
        
        var now = DateTime.UtcNow;
        var isOverdue = homeworkStudent.Assignment.DueAt.HasValue && 
                       now > homeworkStudent.Assignment.DueAt.Value && 
                       (homeworkStudent.Status == HomeworkStatus.Assigned || homeworkStudent.Status == HomeworkStatus.Missing);

        List<HomeworkQuestionDto> questions = new();
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

            var reviewData = await QuizSubmissionReviewBuilder.BuildAsync(
                context,
                homeworkStudent.AssignmentId,
                homeworkStudent.TextAnswer,
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
            AllowResubmit = homeworkStudent.Assignment.AllowResubmit,
            AiHintEnabled = homeworkStudent.Assignment.AiHintEnabled,
            AiRecommendEnabled = homeworkStudent.Assignment.AiRecommendEnabled,
            SpeakingMode = homeworkStudent.Assignment.SpeakingMode,
            TargetWords = StringListJson.Deserialize(homeworkStudent.Assignment.TargetWords),
            SpeakingExpectedText = homeworkStudent.Assignment.SpeakingExpectedText,
            Status = homeworkStudent.Status.ToString(),
            StartedAt = homeworkStudent.StartedAt,
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
            ShowReview = showReview,
            ShowCorrectAnswer = showReview,
            ShowExplanation = showReview,
            TeacherName = homeworkStudent.Assignment.CreatedByUser != null
                ? homeworkStudent.Assignment.CreatedByUser.Name
                : null,
            AssignmentAttachmentUrls = string.IsNullOrWhiteSpace(homeworkStudent.Assignment.AttachmentUrl)
                ? new List<string>()
                : new List<string> { homeworkStudent.Assignment.AttachmentUrl },
            Submission = new HomeworkSubmissionPayloadDto
            {
                TextAnswer = homeworkStudent.TextAnswer,
                AttachmentUrls = homeworkStudent.Assignment.SubmissionType == SubmissionType.Link ||
                                 string.IsNullOrWhiteSpace(homeworkStudent.AttachmentUrl)
                    ? new List<string>()
                    : new List<string> { homeworkStudent.AttachmentUrl },
                Links = !string.IsNullOrWhiteSpace(homeworkStudent.AttachmentUrl) &&
                        homeworkStudent.Assignment.SubmissionType == SubmissionType.Link
                    ? new List<string> { homeworkStudent.AttachmentUrl }
                    : new List<string>(),
                SubmittedAt = homeworkStudent.SubmittedAt,
                GradedAt = homeworkStudent.GradedAt,
                Score = homeworkStudent.Score,
                TeacherFeedback = homeworkStudent.TeacherFeedback
            }
        };
    }
}
