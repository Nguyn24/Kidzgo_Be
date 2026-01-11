using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.Exams.CreateExam;
using Kidzgo.Application.Exams.CreateExamResult;
using Kidzgo.Application.Exams.CreateExamResultsBulk;
using Kidzgo.Application.Exams.DeleteExam;
using Kidzgo.Application.Exams.GetExamById;
using Kidzgo.Application.Exams.GetExamResultById;
using Kidzgo.Application.Exams.GetExamResults;
using Kidzgo.Application.Exams.GetExams;
using Kidzgo.Application.Exams.GetStudentExamResults;
using Kidzgo.Application.Exams.UpdateExam;
using Kidzgo.Application.Exams.UpdateExamResult;
using Kidzgo.Application.Exams.CreateExamQuestion;
using Kidzgo.Application.Exams.GetExamQuestions;
using Kidzgo.Application.Exams.GetExamQuestionById;
using Kidzgo.Application.Exams.UpdateExamQuestion;
using Kidzgo.Application.Exams.DeleteExamQuestion;
using Kidzgo.Application.Exams.StartExamSubmission;
using Kidzgo.Application.Exams.SaveExamSubmissionAnswer;
using Kidzgo.Application.Exams.SubmitExamSubmission;
using Kidzgo.Application.Exams.GetExamSubmissions;
using Kidzgo.Application.Exams.GetExamSubmission;
using Kidzgo.Application.Exams.GradeExamSubmission;
using Kidzgo.Domain.Exams;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/exams")]
[ApiController]
public class ExamController : ControllerBase
{
    private readonly ISender _mediator;

    public ExamController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// UC-152: Tạo Exam cho Class
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Teacher,Staff,Admin")]
    public async Task<IResult> CreateExam(
        [FromBody] CreateExamRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateExamCommand
        {
            ClassId = request.ClassId,
            ExamType = request.ExamType,
            Date = request.Date,
            MaxScore = request.MaxScore,
            Description = request.Description,
            ScheduledStartTime = request.ScheduledStartTime,
            TimeLimitMinutes = request.TimeLimitMinutes,
            AllowLateStart = request.AllowLateStart,
            LateStartToleranceMinutes = request.LateStartToleranceMinutes,
            AutoSubmitOnTimeLimit = request.AutoSubmitOnTimeLimit,
            PreventCopyPaste = request.PreventCopyPaste,
            PreventNavigation = request.PreventNavigation,
            ShowResultsImmediately = request.ShowResultsImmediately
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(e => $"/api/exams/{e.Id}");
    }

    /// <summary>
    /// UC-153: Xem danh sách Exams của Class (filter theo classId)
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IResult> GetExams(
        [FromQuery] Guid? classId,
        [FromQuery] ExamType? examType,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetExamsQuery
        {
            ClassId = classId,
            ExamType = examType,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-154: Xem chi tiết Exam
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IResult> GetExamById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetExamByIdQuery { Id = id };
        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-155: Cập nhật Exam
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Teacher,Staff,Admin")]
    public async Task<IResult> UpdateExam(
        Guid id,
        [FromBody] UpdateExamRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateExamCommand
        {
            Id = id,
            ExamType = request.ExamType,
            Date = request.Date,
            MaxScore = request.MaxScore,
            Description = request.Description,
            ScheduledStartTime = request.ScheduledStartTime,
            TimeLimitMinutes = request.TimeLimitMinutes,
            AllowLateStart = request.AllowLateStart,
            LateStartToleranceMinutes = request.LateStartToleranceMinutes,
            AutoSubmitOnTimeLimit = request.AutoSubmitOnTimeLimit,
            PreventCopyPaste = request.PreventCopyPaste,
            PreventNavigation = request.PreventNavigation,
            ShowResultsImmediately = request.ShowResultsImmediately
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-156: Xóa Exam
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> DeleteExam(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteExamCommand { Id = id };
        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-157: Nhập Exam Result cho 1 học sinh
    /// </summary>
    [HttpPost("{examId:guid}/results")]
    [Authorize(Roles = "Teacher,Staff,Admin")]
    public async Task<IResult> CreateExamResult(
        Guid examId,
        [FromBody] CreateExamResultRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateExamResultCommand
        {
            ExamId = examId,
            StudentProfileId = request.StudentProfileId,
            Score = request.Score,
            Comment = request.Comment,
            AttachmentUrls = request.AttachmentUrls
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(er => $"/api/exams/{examId}/results/{er.Id}");
    }

    /// <summary>
    /// UC-157a: Nhập Exam Results bulk cho nhiều học sinh
    /// </summary>
    [HttpPost("{examId:guid}/results/bulk")]
    [Authorize(Roles = "Teacher,Staff,Admin")]
    public async Task<IResult> CreateExamResultsBulk(
        Guid examId,
        [FromBody] CreateExamResultsBulkRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateExamResultsBulkCommand
        {
            ExamId = examId,
            Results = request.Results.Select(r => new ExamResultItem
            {
                StudentProfileId = r.StudentProfileId,
                Score = r.Score,
                Comment = r.Comment,
                AttachmentUrls = r.AttachmentUrls
            }).ToList()
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-158: Xem danh sách Exam Results
    /// </summary>
    [HttpGet("results")]
    [Authorize]
    public async Task<IResult> GetExamResults(
        [FromQuery] Guid? examId,
        [FromQuery] Guid? studentProfileId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetExamResultsQuery
        {
            ExamId = examId,
            StudentProfileId = studentProfileId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-159: Xem chi tiết Exam Result
    /// </summary>
    [HttpGet("results/{id:guid}")]
    [Authorize]
    public async Task<IResult> GetExamResultById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetExamResultByIdQuery { Id = id };
        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-160: Cập nhật Exam Result
    /// </summary>
    [HttpPut("results/{id:guid}")]
    [Authorize(Roles = "Teacher,Staff,Admin")]
    public async Task<IResult> UpdateExamResult(
        Guid id,
        [FromBody] UpdateExamResultRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateExamResultCommand
        {
            Id = id,
            Score = request.Score,
            Comment = request.Comment,
            AttachmentUrls = request.AttachmentUrls
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-162: Parent/Student xem lịch sử Exam Results của học sinh (filter theo type)
    /// </summary>
    [HttpGet("students/{studentProfileId:guid}")]
    [Authorize]
    public async Task<IResult> GetStudentExamResults(
        Guid studentProfileId,
        [FromQuery] ExamType? examType,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetStudentExamResultsQuery
        {
            StudentProfileId = studentProfileId,
            ExamType = examType,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-163: Tạo Exam Question
    /// </summary>
    [HttpPost("{examId:guid}/questions")]
    [Authorize(Roles = "Teacher,Staff,Admin")]
    public async Task<IResult> CreateExamQuestion(
        Guid examId,
        [FromBody] CreateExamQuestionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateExamQuestionCommand
        {
            ExamId = examId,
            OrderIndex = request.OrderIndex,
            QuestionText = request.QuestionText,
            QuestionType = request.QuestionType,
            Options = request.Options,
            CorrectAnswer = request.CorrectAnswer,
            Points = request.Points,
            Explanation = request.Explanation
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(q => $"/api/exams/{examId}/questions/{q.Id}");
    }

    /// <summary>
    /// UC-164: Xem danh sách Exam Questions của Exam
    /// </summary>
    [HttpGet("{examId:guid}/questions")]
    [Authorize]
    public async Task<IResult> GetExamQuestions(
        Guid examId,
        CancellationToken cancellationToken)
    {
        var query = new GetExamQuestionsQuery { ExamId = examId };
        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-165: Xem chi tiết Exam Question
    /// </summary>
    [HttpGet("{examId:guid}/questions/{questionId:guid}")]
    [Authorize]
    public async Task<IResult> GetExamQuestionById(
        Guid examId,
        Guid questionId,
        CancellationToken cancellationToken)
    {
        var query = new GetExamQuestionByIdQuery { QuestionId = questionId };
        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-166: Cập nhật Exam Question
    /// </summary>
    [HttpPut("{examId:guid}/questions/{questionId:guid}")]
    [Authorize(Roles = "Teacher,Staff,Admin")]
    public async Task<IResult> UpdateExamQuestion(
        Guid examId,
        Guid questionId,
        [FromBody] UpdateExamQuestionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateExamQuestionCommand
        {
            QuestionId = questionId,
            OrderIndex = request.OrderIndex,
            QuestionText = request.QuestionText,
            QuestionType = request.QuestionType,
            Options = request.Options,
            CorrectAnswer = request.CorrectAnswer,
            Points = request.Points,
            Explanation = request.Explanation
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-167: Xóa Exam Question
    /// </summary>
    [HttpDelete("{examId:guid}/questions/{questionId:guid}")]
    [Authorize(Roles = "Teacher,Staff,Admin")]
    public async Task<IResult> DeleteExamQuestion(
        Guid examId,
        Guid questionId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteExamQuestionCommand { QuestionId = questionId };
        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-169: Học sinh bắt đầu làm bài thi
    /// </summary>
    [HttpPost("{examId:guid}/submissions/start")]
    [Authorize]
    public async Task<IResult> StartExamSubmission(
        Guid examId,
        [FromBody] StartExamSubmissionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new StartExamSubmissionCommand
        {
            ExamId = examId,
            StudentProfileId = request.StudentProfileId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(s => $"/api/exams/{examId}/submissions/{s.Id}");
    }

    /// <summary>
    /// UC-170: Học sinh lưu câu trả lời
    /// </summary>
    [HttpPut("{examId:guid}/submissions/{submissionId:guid}/answers")]
    [Authorize]
    public async Task<IResult> SaveExamSubmissionAnswer(
        Guid examId,
        Guid submissionId,
        [FromBody] SaveExamSubmissionAnswerRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SaveExamSubmissionAnswerCommand
        {
            SubmissionId = submissionId,
            QuestionId = request.QuestionId,
            Answer = request.Answer
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-171: Học sinh nộp bài thi
    /// </summary>
    [HttpPost("{examId:guid}/submissions/{submissionId:guid}/submit")]
    [Authorize]
    public async Task<IResult> SubmitExamSubmission(
        Guid examId,
        Guid submissionId,
        CancellationToken cancellationToken)
    {
        var command = new SubmitExamSubmissionCommand
        {
            SubmissionId = submissionId,
            IsAutoSubmit = false
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-174: Xem bài làm của học sinh (Teacher/Admin)
    /// </summary>
    [HttpGet("{examId:guid}/submissions")]
    [Authorize(Roles = "Teacher,Staff,Admin")]
    public async Task<IResult> GetExamSubmissions(
        Guid examId,
        [FromQuery] Guid? studentProfileId,
        [FromQuery] ExamSubmissionStatus? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetExamSubmissionsQuery
        {
            ExamId = examId,
            StudentProfileId = studentProfileId,
            Status = status,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-174: Xem chi tiết bài làm
    /// </summary>
    [HttpGet("{examId:guid}/submissions/{submissionId:guid}")]
    [Authorize]
    public async Task<IResult> GetExamSubmission(
        Guid examId,
        Guid submissionId,
        [FromQuery] bool includeAnswers = true,
        [FromQuery] bool showCorrectAnswers = false,
        CancellationToken cancellationToken = default)
    {
        var query = new GetExamSubmissionQuery
        {
            SubmissionId = submissionId,
            IncludeAnswers = includeAnswers,
            ShowCorrectAnswers = showCorrectAnswers
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-175: Teacher chấm bài thi
    /// </summary>
    [HttpPut("{examId:guid}/submissions/{submissionId:guid}/grade")]
    [Authorize(Roles = "Teacher,Staff,Admin")]
    public async Task<IResult> GradeExamSubmission(
        Guid examId,
        Guid submissionId,
        [FromBody] GradeExamSubmissionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new GradeExamSubmissionCommand
        {
            SubmissionId = submissionId,
            FinalScore = request.FinalScore,
            TeacherComment = request.TeacherComment,
            AnswerGrades = request.AnswerGrades?.Select(g => new GradeAnswerItem
            {
                QuestionId = g.QuestionId,
                PointsAwarded = g.PointsAwarded,
                TeacherFeedback = g.TeacherFeedback
            }).ToList()
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}

