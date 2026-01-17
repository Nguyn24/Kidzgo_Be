using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Profiles.GetProfileById;

public sealed class GetProfileByIdQueryHandler(IDbContext context)
    : IQueryHandler<GetProfileByIdQuery, GetProfileByIdResponse>
{
    public async Task<Result<GetProfileByIdResponse>> Handle(GetProfileByIdQuery query, CancellationToken cancellationToken)
    {
        var profile = await context.Profiles
            .Include(p => p.User)
            .Where(p => p.Id == query.Id)
            .Select(p => new GetProfileByIdResponse
            {
                Id = p.Id,
                UserId = p.UserId,
                UserEmail = p.User.Email,
                ProfileType = p.ProfileType,
                DisplayName = p.DisplayName,
                IsActive = p.IsActive,
                IsDeleted = p.IsDeleted,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (profile is null)
        {
            return Result.Failure<GetProfileByIdResponse>(ProfileErrors.NotFound(query.Id));
        }

        return profile;
    }
}

