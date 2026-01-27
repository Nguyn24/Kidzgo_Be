using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.Missions.CreateMission;
using Kidzgo.Application.Missions.DeleteMission;
using Kidzgo.Application.Missions.GetMissionById;
using Kidzgo.Application.Missions.GetMissionProgress;
using Kidzgo.Application.Missions.GetMissions;
using Kidzgo.Application.Missions.UpdateMission;
using Kidzgo.Domain.Gamification;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/missions")]
[ApiController]
[Authorize]
public class MissionController : ControllerBase
{
    private readonly ISender _mediator;

    public MissionController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// UC-188: Tạo Mission (CLASS/STUDENT/GROUP scope)
    /// UC-193: Thiết lập reward_stars cho Mission
    /// UC-194: Thiết lập reward_exp cho Mission
    [HttpPost]
    [Authorize(Roles = "Admin,ManagementStaff,Teacher")]
    public async Task<IResult> CreateMission(
        [FromBody] CreateMissionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateMissionCommand
        {
            Title = request.Title,
            Description = request.Description,
            Scope = request.Scope,
            TargetClassId = request.TargetClassId,
            TargetGroup = request.TargetGroup,
            MissionType = request.MissionType,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            RewardStars = request.RewardStars,
            RewardExp = request.RewardExp
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(m => $"/api/missions/{m.Id}");
    }

    /// UC-189: Xem danh sách Missions
    [HttpGet]
    [Authorize(Roles = "Admin,ManagementStaff,Teacher,Parent,Student")]
    public async Task<IResult> GetMissions(
        [FromQuery] MissionScope? scope,
        [FromQuery] Guid? targetClassId,
        [FromQuery] string? targetGroup,
        [FromQuery] MissionType? missionType,
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMissionsQuery
        {
            Scope = scope,
            TargetClassId = targetClassId,
            TargetGroup = targetGroup,
            MissionType = missionType,
            SearchTerm = searchTerm,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// UC-190: Xem chi tiết Mission
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,ManagementStaff,Teacher,Parent,Student")]
    public async Task<IResult> GetMissionById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetMissionByIdQuery
        {
            Id = id
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// UC-191: Cập nhật Mission
    /// UC-193: Thiết lập reward_stars cho Mission
    /// UC-194: Thiết lập reward_exp cho Mission
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,ManagementStaff,Teacher")]
    public async Task<IResult> UpdateMission(
        Guid id,
        [FromBody] UpdateMissionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateMissionCommand
        {
            Id = id,
            Title = request.Title,
            Description = request.Description,
            Scope = request.Scope,
            TargetClassId = request.TargetClassId,
            TargetGroup = request.TargetGroup,
            MissionType = request.MissionType,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            RewardStars = request.RewardStars,
            RewardExp = request.RewardExp
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-192: Xóa Mission
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> DeleteMission(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteMissionCommand
        {
            Id = id
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-197: Track progress của Mission
    /// UC-198: Hoàn thành Mission (COMPLETED)
    /// UC-199: Xem progress bar của Mission
    [HttpGet("{id:guid}/progress")]
    [Authorize(Roles = "Admin,ManagementStaff,Teacher,Parent,Student")]
    public async Task<IResult> GetMissionProgress(
        Guid id,
        [FromQuery] Guid? studentProfileId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMissionProgressQuery
        {
            MissionId = id,
            StudentProfileId = studentProfileId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }
}

