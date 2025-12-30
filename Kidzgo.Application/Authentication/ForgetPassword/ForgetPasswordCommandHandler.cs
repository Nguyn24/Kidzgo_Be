using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Events;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Authentication.ForgetPassword;

public sealed class ForgetPasswordCommandHandler(
    IDbContext context
) : ICommandHandler<ForgetPasswordCommand>
{
    public async Task<Result> Handle(ForgetPasswordCommand command, CancellationToken cancellationToken)
    {
        var normalizedEmail = command.Email.Trim().ToLower();

        User? user = await context.Users
            .SingleOrDefaultAsync(u => u.Email.Trim().ToLower() == normalizedEmail, cancellationToken);

        // Để tránh lộ thông tin user tồn tại hay không, luôn trả về Success
        if (user is null)
        {
            return Result.Success();
        }

        user.Raise(new ForgetPasswordDomainEvent(user.Id));

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}


