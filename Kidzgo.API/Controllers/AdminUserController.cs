using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.Users.Admin.CreateUser;
using Kidzgo.Application.Users.Admin.GetAllUser;
using Kidzgo.Application.Users.Admin.GetUserById;
using Kidzgo.Application.Users.Admin.UpdateUser;
using Kidzgo.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;


[Route("api/admin/users")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminUserController : ControllerBase
{
    private readonly ISender _mediator;

    public AdminUserController(ISender mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet]
    public async Task<IResult> GetUsers(
        [FromQuery] bool? isActive,
        [FromQuery] string? role,
        [FromQuery] Guid? branchId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        UserRole? userRole = null;
        if (!string.IsNullOrWhiteSpace(role) && Enum.TryParse<UserRole>(role, true, out var parsedRole))
        {
            userRole = parsedRole;
        }

        var query = new GetUsersQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            IsActive = isActive,
            Role = userRole,
            BranchId = branchId
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

  
    /// <summary>
    /// UC-372: Xem chi tiết User
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IResult> GetUserById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetUserByIdQuery
        {
            Id = id
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

  
    [HttpPost]
    public async Task<IResult> CreateUser(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateUserCommand
        {
            Name = request.Name,
            Email = request.Email,
            Password = request.Password,
            Role = request.Role
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(u => $"/api/admin/users/{u.Id}");
    }
    
    [HttpPut("{id:guid}")]
    public async Task<IResult> UpdateUser(
        Guid id,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateUserCommand
        {
            UserId = id,
            FullName = request.FullName,
            Email = request.Email,
            Role = request.Role,
            isDeleted = request.IsDeleted
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

  
    [HttpDelete("{id:guid}")]
    public async Task<IResult> DeleteUser(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new UpdateUserCommand
        {
            UserId = id,
            isDeleted = true
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}

