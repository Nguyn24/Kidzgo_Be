using Kidzgo.API.Extensions;
using Kidzgo.Application.Authentication.Logout;
using Kidzgo.Application.Users.GetCurrentUser;
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
    /// Logout - remove all refresh tokens
    /// </summary>
    [HttpPost("logout")]
    public async Task<IResult> Logout(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new LogoutCommand(), cancellationToken);
        return result.MatchOk();
    }
}

