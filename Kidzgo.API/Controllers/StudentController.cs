using Kidzgo.API.Extensions;
using Kidzgo.API.Infrastructure;
using Kidzgo.API.Requests;
using Kidzgo.Application.Classes.GetStudentClasses;
using Kidzgo.Application.Homework.GetStudentHomeworks;
using Kidzgo.Application.Homework.GetStudentHomeworkFeedback;
using Kidzgo.Application.Homework.GetStudentHomeworkSubmission;
using Kidzgo.Application.Homework.SubmitHomework;
using Kidzgo.Application.Homework.SubmitMultipleChoiceHomework;
using Kidzgo.Application.Sessions.GetStudentTimetable;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.LessonPlans;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/students")]
[ApiController]
[Authorize]
public class StudentController : ControllerBase
{
    private readonly ISender _mediator;

    public StudentController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// Lấy danh sách lớp của Student (studentId lấy từ token)
    [HttpGet("classes")]
    public async Task<IResult> GetClasses(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetStudentClassesQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// Lấy thời khóa biểu của Student (studentId lấy từ token)
    [HttpGet("timetable")]
    public async Task<IResult> GetTimetable(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken = default)
    {
        var query = new GetStudentTimetableQuery
        {
            From = from,
            To = to
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

   
    /// UC-125: Học sinh nộp Homework
    [HttpPost("homework/submit")]
    public async Task<IResult> SubmitHomework(
        [FromBody] SubmitHomeworkRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SubmitHomeworkCommand
        {
            HomeworkStudentId = request.HomeworkStudentId,
            TextAnswer = request.TextAnswer, 
            AttachmentUrls = request.AttachmentUrls, 
            LinkUrl = request.LinkUrl 
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Học sinh nộp Multiple Choice Homework
    /// </summary>
    [HttpPost("homework/multiple-choice/submit")]
    public async Task<IResult> SubmitMultipleChoiceHomework(
        [FromBody] SubmitMultipleChoiceHomeworkRequest request,
        CancellationToken cancellationToken)
    {
        var answers = request.Answers.Select(a => new SubmitAnswerDto
        {
            QuestionId = a.QuestionId,
            SelectedOptionId = a.SelectedOptionId
        }).ToList();

        var command = new SubmitMultipleChoiceHomeworkCommand
        {
            HomeworkStudentId = request.HomeworkStudentId,
            Answers = answers
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-126: Xem danh sách Homework của bản thân
    /// </summary>
    [HttpGet("homework/my")]
    public async Task<IResult> GetHomeworks(
        [FromQuery] string? status,
        [FromQuery] Guid? classId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        HomeworkStatus? parsedStatus = null;
        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<HomeworkStatus>(status, ignoreCase: true, out var tmpStatus))
        {
            parsedStatus = tmpStatus;
        }

        var query = new GetStudentHomeworksQuery
        {
            Status = parsedStatus,
            ClassId = classId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-126a: Xem danh sách Homework đã nộp (bao gồm Submitted và Graded)
    /// </summary>
    [HttpGet("homework/submitted")]
    public async Task<IResult> GetSubmittedHomeworks(
        [FromQuery] Guid? classId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        // Get all homeworks first, then filter for Submitted or Graded
        var query = new GetStudentHomeworksQuery
        {
            Status = null, // Get all statuses, we'll filter in handler
            ClassId = classId,
            PageNumber = 1, // Get all to filter properly
            PageSize = 1000 // Large page size to get all, then filter
        };

        var result = await _mediator.Send(query, cancellationToken);
        
        if (!result.IsSuccess)
        {
            return result.MatchOk();
        }
        
        // Filter to only include Submitted and Graded
        var submittedAndGraded = result.Value.Homeworks.Items
            .Where(h => h.Status == HomeworkStatus.Submitted.ToString() || 
                       h.Status == HomeworkStatus.Graded.ToString())
            .ToList();
        
        // Apply pagination after filtering
        var totalCount = submittedAndGraded.Count;
        var pagedItems = submittedAndGraded
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        
        var filteredPage = new Page<StudentHomeworkDto>(
            pagedItems,
            totalCount,
            pageNumber,
            pageSize);
        
        var filteredResponse = new GetStudentHomeworksResponse
        {
            Homeworks = filteredPage
        };
        
        return Results.Ok(ApiResult<GetStudentHomeworksResponse>.Success(filteredResponse));
    }

    /// <summary>
    /// UC-127: Xem chi tiết Homework submission
    /// </summary>
    [HttpGet("homework/{homeworkStudentId:guid}")]
    public async Task<IResult> GetHomeworkSubmission(
        Guid homeworkStudentId,
        CancellationToken cancellationToken)
    {
        var query = new GetStudentHomeworkSubmissionQuery
        {
            HomeworkStudentId = homeworkStudentId
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Xem điểm và feedback của bản thân
    /// </summary>
    [HttpGet("homework/feedback/my")]
    public async Task<IResult> GetMyHomeworkFeedback(
        [FromQuery] Guid? classId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetStudentHomeworkFeedbackQuery
        {
            ClassId = classId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }
}

