using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Branches.GetBranches;

public sealed class GetBranchesQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetBranchesQuery, GetBranchesResponse>
{
    public async Task<Result<GetBranchesResponse>> Handle(GetBranchesQuery query, CancellationToken cancellationToken)
    {
        // Get current user to check role
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (user == null)
        {
            return Result.Failure<GetBranchesResponse>(
                Error.NotFound("User.NotFound", "User not found"));
        }

        IQueryable<Domain.Schools.Branch> branchesQuery = context.Branches;

        // Admin sees all branches, Staff/Teacher see only their branch
        if (user.Role != UserRole.Admin)
        {
            if (user.BranchId.HasValue)
            {
                branchesQuery = branchesQuery.Where(b => b.Id == user.BranchId.Value);
            }
            else
            {
                // User has no branch assigned, return empty list
                return Result.Success(new GetBranchesResponse
                {
                    Branches = new List<BranchDto>()
                });
            }
        }

        var branches = await branchesQuery
            .Where(b => b.IsActive)
            .OrderBy(b => b.Name)
            .Select(b => new BranchDto
            {
                Id = b.Id,
                Code = b.Code,
                Name = b.Name,
                Address = b.Address,
                ContactPhone = b.ContactPhone,
                ContactEmail = b.ContactEmail,
                IsActive = b.IsActive
            })
            .ToListAsync(cancellationToken);

        return Result.Success(new GetBranchesResponse
        {
            Branches = branches
        });
    }
}

