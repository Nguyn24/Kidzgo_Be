using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
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
        
        // var currentUserResult = await sender.Send(new GetCurrentUserQuery(), cancellationToken);
        // var currentUser = currentUserResult.Value;
        // bool isStaff = currentUser.Role == UserRole.Staff.ToString();
        

        user.Username = request.FullName ?? user.Username;
        user.Email = request.Email ?? user.Email;

        // if (isStaff)
        // 
            user.Role = request.Role ?? user.Role;
            user.IsDeleted = request.isDeleted ?? user.IsDeleted;
        // }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new UpdateUserResponse(user));
    }
}
