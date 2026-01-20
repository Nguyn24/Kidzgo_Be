using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Users.Admin.UpdateUser;

public sealed class UpdateUserCommandHandler(IDbContext context, IUserContext userContext, ISender sender)
    : ICommandHandler<UpdateUserCommand, UpdateUserResponse>
{
    public async Task<Result<UpdateUserResponse>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null)
        {
            return Result.Failure<UpdateUserResponse>(UserErrors.NotFound(request.UserId));
        }
        
        // Parse and validate role if provided
        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
            {
                return Result.Failure<UpdateUserResponse>(UserErrors.InvalidRole(request.Role));
            }
            user.Role = role;
        }

        user.Username = request.Username ?? user.Username;
        user.Name = request.FullName ?? user.Name;
        user.Email = request.Email ?? user.Email;
        user.IsActive = request.IsActive ?? user.IsActive;
        user.IsDeleted = request.isDeleted ?? user.IsDeleted;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new UpdateUserResponse(user));
    }
}
