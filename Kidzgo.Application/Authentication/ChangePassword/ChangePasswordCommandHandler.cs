using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Authentication.ChangePassword
{
    public class ChangePasswordCommandHandler(IDbContext context, IUserContext userContext,
         IPasswordHasher passwordHasher
         ) : ICommandHandler<ChangePasswordCommand>
    {
        public async Task<Result> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

            if (user is null)
            {
                return Result.Failure<ChangePasswordCommand>(UserErrors.NotFound(userContext.UserId));
            }

            if (!passwordHasher.Verify(command.CurrentPassword, user.PasswordHash))
            {
                return Result.Failure<ChangePasswordCommand>(UserErrors.InvalidCurrentPassword);
            }

            if (command.CurrentPassword == command.NewPassword)
            {
                return Result.Failure<ChangePasswordCommand>(UserErrors.SamePassword);
            }

            
            user.PasswordHash = passwordHasher.Hash(command.NewPassword);

            await context.SaveChangesAsync(cancellationToken);


            return Result.Success();
        }
    }
}
