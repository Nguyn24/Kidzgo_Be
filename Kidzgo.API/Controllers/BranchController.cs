using Kidzgo.API.Extensions;
using Kidzgo.Application.Branches.GetBranches;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/branches")]
[ApiController]
public class BranchController : ControllerBase
{
    private readonly ISender _mediator;

    public BranchController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// UC-385: Xem danh sách Branches (Admin thấy all; Staff/Teacher thấy 1)
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IResult> GetBranches(CancellationToken cancellationToken)
    {
        var query = new GetBranchesQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }
}

