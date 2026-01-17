using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Users.Admin.GetAllUser;

public sealed class GetUsersQueryHandler(IDbContext context)
    : IQueryHandler<GetUsersQuery, Page<GetUsersResponse>>
{
    public async Task<Result<Page<GetUsersResponse>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var query = context.Users
            .Include(u => u.Branch)
            .Include(u => u.Profiles.Where(p => !p.IsDeleted))
            .Where(u => !u.IsDeleted);

        // Apply filters
        if (request.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == request.IsActive.Value);
        }

        if (request.Role.HasValue)
        {
            query = query.Where(u => u.Role == request.Role.Value);
        }

        if (request.BranchId.HasValue)
        {
            query = query.Where(u => u.BranchId == request.BranchId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .ApplyPagination(request.PageNumber, request.PageSize)
            .ToListAsync(cancellationToken);

        var result = users.Select(u => new GetUsersResponse
        {
            Id = u.Id,
            Username = u.Username,
            Name = u.Name,
            Email = u.Email,
            Role = u.Role,
            BranchId = u.BranchId,
            BranchName = u.Branch != null ? u.Branch.Name : null,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt,
            UpdatedAt = u.UpdatedAt,
            Profiles = u.Profiles
                .Where(p => !p.IsDeleted)
                .Select(p => new UserProfileDto
                {
                    Id = p.Id,
                    ProfileType = p.ProfileType,
                    DisplayName = p.DisplayName,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt
                })
                .ToList()
        }).ToList();

        return new Page<GetUsersResponse>(
            result,
            totalCount,
            request.PageNumber,
            request.PageSize);
    }
}