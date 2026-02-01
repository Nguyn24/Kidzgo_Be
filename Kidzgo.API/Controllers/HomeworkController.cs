using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.Homework.CreateHomeworkAssignment;
using Kidzgo.Application.Homework.DeleteHomeworkAssignment;
using Kidzgo.Application.Homework.GetHomeworkAssignmentById;
using Kidzgo.Application.Homework.GetHomeworkAssignments;
using Kidzgo.Application.Homework.GetStudentHomeworkHistory;
using Kidzgo.Application.Homework.GradeHomework;
using Kidzgo.Application.Homework.LinkHomeworkToMission;
using Kidzgo.Application.Homework.MarkHomeworkLateOrMissing;
using Kidzgo.Application.Homework.SetHomeworkRewardStars;
using Kidzgo.Application.Homework.UpdateHomeworkAssignment;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/homework")]
[ApiController]
[Authorize]
public class HomeworkController : ControllerBase
{
    private readonly ISender _mediator;

    public HomeworkController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// UC-117: Tạo Homework Assignment
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> CreateHomeworkAssignment(
        [FromBody] CreateHomeworkAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<SubmissionType>(request.SubmissionType, ignoreCase: true, out var submissionType))
        {
            return Results.BadRequest(HomeworkErrors.InvalidSubmissionType);
        }

        var command = new CreateHomeworkAssignmentCommand
        {
            ClassId = request.ClassId,
            SessionId = request.SessionId,
            Title = request.Title,
            Description = request.Description,
            DueAt = request.DueAt,
            Book = request.Book,
            Pages = request.Pages,
            Skills = request.Skills,
            SubmissionType = submissionType,
            MaxScore = request.MaxScore,
            RewardStars = request.RewardStars,
            MissionId = request.MissionId,
            Instructions = request.Instructions,
            ExpectedAnswer = request.ExpectedAnswer,
            Rubric = request.Rubric
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(r => $"/api/homework/{r.Id}");
    }

    /// <summary>
    /// UC-118: Xem danh sách Homework Assignments
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> GetHomeworkAssignments(
        [FromQuery] Guid? classId,
        [FromQuery] Guid? sessionId,
        [FromQuery] string? skill,
        [FromQuery] string? submissionType,
        [FromQuery] Guid? branchId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        SubmissionType? parsedSubmissionType = null;
        if (!string.IsNullOrWhiteSpace(submissionType) &&
            Enum.TryParse<SubmissionType>(submissionType, ignoreCase: true, out var tmpType))
        {
            parsedSubmissionType = tmpType;
        }

        var query = new GetHomeworkAssignmentsQuery
        {
            ClassId = classId,
            SessionId = sessionId,
            Skill = skill,
            SubmissionType = parsedSubmissionType,
            BranchId = branchId,
            FromDate = fromDate,
            ToDate = toDate,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-119: Xem chi tiết Homework Assignment
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> GetHomeworkAssignmentById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetHomeworkAssignmentByIdQuery { Id = id };
        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-120: Cập nhật Homework Assignment
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> UpdateHomeworkAssignment(
        Guid id,
        [FromBody] UpdateHomeworkAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        SubmissionType? parsedSubmissionType = null;
        if (!string.IsNullOrWhiteSpace(request.SubmissionType) &&
            Enum.TryParse<SubmissionType>(request.SubmissionType, ignoreCase: true, out var tmpType))
        {
            parsedSubmissionType = tmpType;
        }

        var command = new UpdateHomeworkAssignmentCommand
        {
            Id = id,
            Title = request.Title,
            Description = request.Description,
            DueAt = request.DueAt,
            Book = request.Book,
            Pages = request.Pages,
            Skills = request.Skills,
            SubmissionType = parsedSubmissionType,
            MaxScore = request.MaxScore,
            RewardStars = request.RewardStars,
            MissionId = request.MissionId,
            Instructions = request.Instructions,
            ExpectedAnswer = request.ExpectedAnswer,
            Rubric = request.Rubric
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-121: Xóa Homework Assignment
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> DeleteHomeworkAssignment(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteHomeworkAssignmentCommand { Id = id };
        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-123: Gắn Homework với Mission
    /// </summary>
    [HttpPost("{id:guid}/link-mission")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> LinkHomeworkToMission(
        Guid id,
        [FromBody] LinkHomeworkToMissionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LinkHomeworkToMissionCommand
        {
            HomeworkId = id,
            MissionId = request.MissionId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-124: Thiết lập reward stars cho Homework
    /// </summary>
    [HttpPut("{id:guid}/reward-stars")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> SetHomeworkRewardStars(
        Guid id,
        [FromBody] SetHomeworkRewardStarsRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SetHomeworkRewardStarsCommand
        {
            HomeworkId = id,
            RewardStars = request.RewardStars
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-128 & UC-129: Teacher chấm Homework và nhập điểm/feedback
    /// </summary>
    [HttpPost("submissions/{homeworkStudentId:guid}/grade")]
    [Authorize(Roles = "Teacher,TeachingAssistant,ManagementStaff,Admin")]
    public async Task<IResult> GradeHomework(
        Guid homeworkStudentId,
        [FromBody] GradeHomeworkRequest request,
        CancellationToken cancellationToken)
    {
        var command = new GradeHomeworkCommand
        {
            HomeworkStudentId = homeworkStudentId,
            Score = request.Score,
            TeacherFeedback = request.TeacherFeedback
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-132: Đánh dấu Homework quá hạn (LATE/MISSING)
    /// </summary>
    [HttpPut("submissions/{homeworkStudentId:guid}/mark-status")]
    [Authorize(Roles = "Teacher,TeachingAssistant,ManagementStaff,Admin")]
    public async Task<IResult> MarkHomeworkLateOrMissing(
        Guid homeworkStudentId,
        [FromBody] MarkHomeworkLateOrMissingRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<HomeworkStatus>(request.Status, ignoreCase: true, out var status) ||
            (status != HomeworkStatus.Late && status != HomeworkStatus.Missing))
        {
            return Results.BadRequest(HomeworkErrors.InvalidStatusForMarking);
        }

        var command = new MarkHomeworkLateOrMissingCommand
        {
            HomeworkStudentId = homeworkStudentId,
            Status = status
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-133: Xem lịch sử Homework của học sinh
    /// </summary>
    [HttpGet("students/{studentProfileId:guid}/history")]
    [Authorize(Roles = "Teacher,TeachingAssistant,ManagementStaff,Admin")]
    public async Task<IResult> GetStudentHomeworkHistory(
        Guid studentProfileId,
        [FromQuery] Guid? classId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetStudentHomeworkHistoryQuery
        {
            StudentProfileId = studentProfileId,
            ClassId = classId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }
}

