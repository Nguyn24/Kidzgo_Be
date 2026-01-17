using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Schools;
using Kidzgo.Domain.Schools.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Branches.CreateBranch;

public sealed class CreateBranchCommandHandler(
    IDbContext context
) : ICommandHandler<CreateBranchCommand, CreateBranchResponse>
{
    public async Task<Result<CreateBranchResponse>> Handle(CreateBranchCommand command, CancellationToken cancellationToken)
    {
        // Check if code already exists
        bool codeExists = await context.Branches
            .AnyAsync(b => b.Code.Trim().ToLower() == command.Code.Trim().ToLower(), cancellationToken);

        if (codeExists)
        {
            return Result.Failure<CreateBranchResponse>(BranchErrors.CodeNotUnique);
        }

        // Check if name already exists
        bool nameExists = await context.Branches
            .AnyAsync(b => b.Name.Trim().ToLower() == command.Name.Trim().ToLower(), cancellationToken);

        if (nameExists)
        {
            return Result.Failure<CreateBranchResponse>(BranchErrors.NameNotUnique);
        }

        var now = DateTime.UtcNow;
        var branch = new Branch
        {
            Id = Guid.NewGuid(),
            Code = command.Code.Trim(),
            Name = command.Name.Trim(),
            Address = command.Address?.Trim(),
            ContactPhone = command.ContactPhone?.Trim(),
            ContactEmail = command.ContactEmail?.Trim(),
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Branches.Add(branch);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateBranchResponse
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

