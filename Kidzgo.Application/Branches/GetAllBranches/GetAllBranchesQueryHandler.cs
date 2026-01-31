using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Branches.GetAllBranches;

public sealed class GetAllBranchesQueryHandler(
    IDbContext context
) : IQueryHandler<GetAllBranchesQuery, GetAllBranchesResponse>
{
    public async Task<Result<GetAllBranchesResponse>> Handle(
        GetAllBranchesQuery query,
        CancellationToken cancellationToken)
    {
        // Get all branches without role-based filtering
        var branches = await context.Branches
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

        return Result.Success(new GetAllBranchesResponse
        {
            Branches = branches
        });
    }
}

