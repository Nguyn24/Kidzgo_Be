using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Profiles.Admin.GetAllProfiles;

public sealed class GetAllProfilesQueryHandler(IDbContext context)
    : IQueryHandler<GetAllProfilesQuery, Page<GetAllProfilesResponse>>
{
    public async Task<Result<Page<GetAllProfilesResponse>>> Handle(GetAllProfilesQuery request, CancellationToken cancellationToken)
    {
        var query = context.Profiles
            .Include(p => p.User)
            .Where(p => !p.IsDeleted)
            .AsQueryable();

        // Apply filters
        if (request.UserId.HasValue)
        {
            query = query.Where(p => p.UserId == request.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.ProfileType) && 
            Enum.TryParse<ProfileType>(request.ProfileType, true, out var profileType))
        {
            query = query.Where(p => p.ProfileType == profileType);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(p => p.IsActive == request.IsActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var result = await query
            .OrderByDescending(p => p.CreatedAt)
            .ApplyPagination(request.PageNumber, request.PageSize)
            .Select(p => new GetAllProfilesResponse
            {
                Id = p.Id,
                UserId = p.UserId,
                UserEmail = p.User.Email,
                ProfileType = p.ProfileType,
                DisplayName = p.DisplayName,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return new Page<GetAllProfilesResponse>(
            result,
            totalCount,
            request.PageNumber,
            request.PageSize);
    }
}

