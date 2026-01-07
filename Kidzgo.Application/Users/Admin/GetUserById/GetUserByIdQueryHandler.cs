using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Users.Admin.GetUserById;

public sealed class GetUserByIdQueryHandler(IDbContext context)
    : IQueryHandler<GetUserByIdQuery, GetUserByIdResponse>
{
    public async Task<Result<GetUserByIdResponse>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .Include(u => u.Branch)
            .Where(u => u.Id == query.Id)
            .Select(u => new GetUserByIdResponse
            {
                Id = u.Id,
                Username = u.Username,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role,
                BranchId = u.BranchId,
                BranchCode = u.Branch != null ? u.Branch.Code : null,
                BranchName = u.Branch != null ? u.Branch.Name : null,
                BranchAddress = u.Branch != null ? u.Branch.Address : null,
                BranchContactPhone = u.Branch != null ? u.Branch.ContactPhone : null,
                BranchContactEmail = u.Branch != null ? u.Branch.ContactEmail : null,
                IsActive = u.IsActive,
                IsDeleted = u.IsDeleted,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return Result.Failure<GetUserByIdResponse>(UserErrors.NotFound(query.Id));
        }

        return user;
    }
}

