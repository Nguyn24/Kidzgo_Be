using Kidzgo.API.Extensions;
using Kidzgo.Application.Sessions.GetSessionById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/sessions")]
[ApiController]
[Authorize]
public class SessionController : ControllerBase
{
    private readonly ISender _mediator;

    public SessionController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// UC-078: Xem chi tiết Session
    /// </summary>
    [HttpGet("{sessionId:guid}")]
    public async Task<IResult> GetSessionById(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var query = new GetSessionByIdQuery
        {
            SessionId = sessionId
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }
}

