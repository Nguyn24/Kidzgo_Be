using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Users.Admin.CreateUser;

public class CreateUserCommandHandler(
    IDbContext context,
    IPasswordHasher passwordHasher
) : ICommandHandler<CreateUserCommand, CreateUserCommandResponse>
{
    public async Task<Result<CreateUserCommandResponse>> Handle(CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        if (await context.Users.AnyAsync(u => u.Email.Trim().ToLower() == command.Email.Trim().ToLower(),
                cancellationToken))
        {
            return Result.Failure<CreateUserCommandResponse>(UserErrors.EmailNotUnique);
        }

        if (command.Role == UserRole.Admin)
        {
            bool adminExists = await context.Users
                .AnyAsync(u => u.Role == UserRole.Admin, cancellationToken);

            if (adminExists)
            {
                return Result.Failure<CreateUserCommandResponse>(UserErrors.AdminAlreadyExists);
            }
        }

        var hashedPassword = passwordHasher.Hash(command.Password);
        
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = command.Name,
            Email = command.Email,
            PasswordHash = hashedPassword,
            Role = command.Role,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        
        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateUserCommandResponse
        {
            Id = user.Id,
            Username = user.Username,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            BranchId = user.BranchId,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}