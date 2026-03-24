using Kidzgo.API.Extensions;
using Kidzgo.API.Infrastructure;
using Kidzgo.API.Requests;
using Kidzgo.Application.Profiles.Admin.GetAllProfiles;
using Kidzgo.Application.Profiles.Admin.ChangeParentPin;
using Kidzgo.Application.Profiles.ApproveProfile;
using Kidzgo.Application.Profiles.CreateProfile;
using Kidzgo.Application.Profiles.DeleteProfile;
using Kidzgo.Application.Profiles.GetProfileById;
using Kidzgo.Application.Profiles.LinkParentStudent;
using Kidzgo.Application.Profiles.ReactivateProfile;
using Kidzgo.Application.Profiles.UnlinkParentStudent;
using Kidzgo.Application.Profiles.UpdateProfile;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/profiles")]
[ApiController]
[Authorize]
public class ProfileController : ControllerBase
{
    private const string FrontendUrl = "https://kidzgo-centre-pvjj.vercel.app/vi";
    private readonly ISender _mediator;

    public ProfileController(ISender mediator)
    {
        _mediator = mediator;
    }

  
    [HttpPost]
    public async Task<IResult> CreateProfile(
        [FromBody] CreateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateProfileCommand
        {
            UserId = request.UserId,
            ProfileType = request.ProfileType,
            DisplayName = request.DisplayName,
            PinHash = request.PinHash
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(p => $"/api/admin/profiles/{p.Id}");
    }

 
    /// <param name="userId">Filter by user ID</param>
    /// <param name="profileType">Filter by profile type</param>
    /// <param name="searchTerm">Search by profile display name</param>
    /// <param name="branchId">Filter by branch ID (for students)</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    [HttpGet]
    public async Task<IResult> GetProfiles(
        [FromQuery] Guid? userId,
        [FromQuery] string? profileType,
        [FromQuery] string? searchTerm,
        [FromQuery] Guid? branchId,
        [FromQuery] bool? isActive,
        [FromQuery] bool? isDeleted,
        [FromQuery] bool? isApproved,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllProfilesQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            UserId = userId,
            ProfileType = profileType,
            SearchTerm = searchTerm,
            BranchId = branchId,
            IsActive = isActive,
            IsDeleted = isDeleted,
            IsApproved = isApproved,
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    
    [HttpGet("{id:guid}")]
    public async Task<IResult> GetProfileById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetProfileByIdQuery
        {
            Id = id
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }


    [HttpPut("{id:guid}")]
    public async Task<IResult> UpdateProfile(
        Guid id,
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProfileCommand
        {
            Id = id,
            DisplayName = request.DisplayName,
            IsActive = request.IsActive
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }


    [HttpDelete("{id:guid}")]
    public async Task<IResult> DeleteProfile(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteProfileCommand
        {
            Id = id
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Reactivate a soft-deleted profile
    /// </summary>
    [HttpPut("{id:guid}/reactivate")]
    public async Task<IResult> ReactivateProfile(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new ReactivateProfileCommand
        {
            Id = id
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("{id:guid}/reactivate-and-update")]
    public async Task<IResult> ReactivateAndRedirectToUpdate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new ReactivateProfileCommand
        {
            Id = id
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure && result.Error.Code != ProfileErrors.ProfileNotDeleted.Code)
        {
            return CustomResults.Problem(result);
        }
        //để đây sửa lại endpoint sau
        return Results.Redirect($"{FrontendUrl}/profile/update?profileId={id}");
    }
    [HttpPost("link")]
    public async Task<IResult> LinkParentStudent(
        [FromBody] LinkParentStudentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LinkParentStudentCommand
        {
            ParentProfileId = request.ParentProfileId,
            StudentProfileId = request.StudentProfileId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(l => $"/api/admin/profiles/link/{l.Id}");
    }

 
    [HttpPost("unlink")]
    public async Task<IResult> UnlinkParentStudent(
        [FromBody] UnlinkParentStudentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UnlinkParentStudentCommand
        {
            ParentProfileId = request.ParentProfileId,
            StudentProfileId = request.StudentProfileId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}
