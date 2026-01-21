using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.Authentication.Logout;
using Kidzgo.Application.Users.GetCurrentUser;
using Kidzgo.Application.Users.UpdateCurrentUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/me")]
[ApiController]
[Authorize]
public class UserController : ControllerBase
{
    private readonly ISender _mediator;

    public UserController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get current user information with role, branchId, permissions, and selected profile
    /// </summary>
    [HttpGet]
    public async Task<IResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCurrentUserQuery(), cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Update current user information and profiles
    /// </summary>
    [HttpPut]
    public async Task<IResult> UpdateCurrentUser(
        [FromBody] UpdateCurrentUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCurrentUserCommand
        {
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            AvatarUrl = request.AvatarUrl,
            Profiles = request.Profiles?.Select(p => new UpdateProfileDto
            {
                Id = p.Id,
                DisplayName = p.DisplayName
            }).ToList()
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Logout - remove all refresh tokens
    /// </summary>
    [HttpPost("logout")]
    public async Task<IResult> Logout(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new LogoutCommand(), cancellationToken);
        return result.MatchOk();
    }
}

