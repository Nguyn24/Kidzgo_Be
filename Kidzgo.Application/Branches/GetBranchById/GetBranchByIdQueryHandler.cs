using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Schools;
using Kidzgo.Domain.Schools.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Branches.GetBranchById;

public sealed class GetBranchByIdQueryHandler(IDbContext context)
    : IQueryHandler<GetBranchByIdQuery, GetBranchByIdResponse>
{
    public async Task<Result<GetBranchByIdResponse>> Handle(GetBranchByIdQuery query, CancellationToken cancellationToken)
    {
        var branch = await context.Branches
            .Where(b => b.Id == query.Id)
            .Select(b => new GetBranchByIdResponse
            {
                Id = b.Id,
                Code = b.Code,
                Name = b.Name,
                Address = b.Address,
                ContactPhone = b.ContactPhone,
                ContactEmail = b.ContactEmail,
                IsActive = b.IsActive,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (branch is null)
        {
            return Result.Failure<GetBranchByIdResponse>(BranchErrors.NotFound(query.Id));
        }

        return branch;
    }
}

