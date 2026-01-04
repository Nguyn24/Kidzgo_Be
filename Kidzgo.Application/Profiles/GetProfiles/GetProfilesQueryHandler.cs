using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Profiles.GetProfiles;

public sealed class GetProfilesQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetProfilesQuery, List<GetProfilesResponse>>
{
    public async Task<Result<List<GetProfilesResponse>>> Handle(GetProfilesQuery request, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        List<Profile> profiles = await context.Profiles
            .Where(p => p.UserId == userId && !p.IsDeleted && p.IsActive)
            .ToListAsync(cancellationToken);

        var response = profiles
            .Select(p => new GetProfilesResponse
            {
                Id = p.Id,
                DisplayName = p.DisplayName,
                ProfileType = p.ProfileType
            })
            .ToList();

        return response;
    }
}







