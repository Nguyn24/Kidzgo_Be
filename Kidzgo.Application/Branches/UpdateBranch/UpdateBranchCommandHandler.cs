using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Schools;
using Kidzgo.Domain.Schools.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Branches.UpdateBranch;

public sealed class UpdateBranchCommandHandler(IDbContext context)
    : ICommandHandler<UpdateBranchCommand, UpdateBranchResponse>
{
    public async Task<Result<UpdateBranchResponse>> Handle(UpdateBranchCommand command, CancellationToken cancellationToken)
    {
        var branch = await context.Branches
            .FirstOrDefaultAsync(b => b.Id == command.Id, cancellationToken);

        if (branch is null)
        {
            return Result.Failure<UpdateBranchResponse>(BranchErrors.NotFound(command.Id));
        }

        // Check if code already exists (if changed)
        if (!string.IsNullOrWhiteSpace(command.Code) && command.Code.Trim().ToLower() != branch.Code.Trim().ToLower())
        {
            bool codeExists = await context.Branches
                .AnyAsync(b => b.Id != command.Id && b.Code.Trim().ToLower() == command.Code.Trim().ToLower(), cancellationToken);

            if (codeExists)
            {
                return Result.Failure<UpdateBranchResponse>(BranchErrors.CodeNotUnique);
            }
            branch.Code = command.Code.Trim();
        }

        // Check if name already exists (if changed)
        if (!string.IsNullOrWhiteSpace(command.Name) && command.Name.Trim().ToLower() != branch.Name.Trim().ToLower())
        {
            bool nameExists = await context.Branches
                .AnyAsync(b => b.Id != command.Id && b.Name.Trim().ToLower() == command.Name.Trim().ToLower(), cancellationToken);

            if (nameExists)
            {
                return Result.Failure<UpdateBranchResponse>(BranchErrors.NameNotUnique);
            }
            branch.Name = command.Name.Trim();
        }

        if (command.Address != null)
        {
            branch.Address = string.IsNullOrWhiteSpace(command.Address) ? null : command.Address.Trim();
        }

        if (command.ContactPhone != null)
        {
            branch.ContactPhone = string.IsNullOrWhiteSpace(command.ContactPhone) ? null : command.ContactPhone.Trim();
        }

        if (command.ContactEmail != null)
        {
            branch.ContactEmail = string.IsNullOrWhiteSpace(command.ContactEmail) ? null : command.ContactEmail.Trim();
        }

        branch.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateBranchResponse
        {
            Id = branch.Id,
            Code = branch.Code,
            Name = branch.Name,
            Address = branch.Address,
            ContactPhone = branch.ContactPhone,
            ContactEmail = branch.ContactEmail,
            IsActive = branch.IsActive,
            CreatedAt = branch.CreatedAt,
            UpdatedAt = branch.UpdatedAt
        };
    }
}

