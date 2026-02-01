using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Schools.Errors;
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
        // Parse and validate role
        if (string.IsNullOrWhiteSpace(command.Role))
        {
            return Result.Failure<CreateUserCommandResponse>(UserErrors.RoleRequired);
        }

        if (!Enum.TryParse<UserRole>(command.Role, true, out var role))
        {
            return Result.Failure<CreateUserCommandResponse>(UserErrors.InvalidRole(command.Role));
        }

        if (await context.Users.AnyAsync(u => u.Email.Trim().ToLower() == command.Email.Trim().ToLower(),
                cancellationToken))
        {
            return Result.Failure<CreateUserCommandResponse>(UserErrors.EmailNotUnique);
        }

        if (role == UserRole.Admin)
        {
            bool adminExists = await context.Users
                .AnyAsync(u => u.Role == UserRole.Admin, cancellationToken);

            if (adminExists)
            {
                return Result.Failure<CreateUserCommandResponse>(UserErrors.AdminAlreadyExists);
            }
        }

        if (role == UserRole.ManagementStaff || role == UserRole.AccountantStaff || role == UserRole.Teacher || role == UserRole.Parent)
        {
            if (!command.BranchId.HasValue)
            {
                return Result.Failure<CreateUserCommandResponse>(UserErrors.BranchRequiredForRole);
            }

            var branchExists = await context.Branches
                .AnyAsync(b => b.Id == command.BranchId.Value, cancellationToken);

            if (!branchExists)
            {
                return Result.Failure<CreateUserCommandResponse>(BranchErrors.NotFound(command.BranchId));
            }
        }

        var hashedPassword = passwordHasher.Hash(command.Password);
        
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = command.Username,
            Name = command.Name,
            Email = command.Email,
            PhoneNumber = string.IsNullOrWhiteSpace(command.PhoneNumber) ? null : command.PhoneNumber.Trim(),
            PasswordHash = hashedPassword,
            Role = role,
            BranchId = command.BranchId,
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
            FullName = user.Name,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role.ToString(),
            BranchId = user.BranchId,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}
