using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Users.GetCurrentUser
{
    public class GetCurrentUserQueryHandler(IDbContext context, IUserContext userContext) : IQueryHandler<GetCurrentUserQuery, GetCurrentUserResponse>
    {
        public async Task<Result<GetCurrentUserResponse>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            var currentUserId = userContext.UserId;

            var user = await context.Users
                .Include(u => u.Branch)
                .Include(u => u.Profiles.Where(p => !p.IsDeleted && p.IsActive))
                .FirstOrDefaultAsync(p => p.Id == currentUserId, cancellationToken);

            if (user == null)
                return Result.Failure<GetCurrentUserResponse>(UserErrors.NotFound(currentUserId));

            var response = new GetCurrentUserResponse
            {
                Id = user.Id,
                UserName = user.Username,
                FullName = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role.ToString(),
                BranchId = user.BranchId,
                Branch = user.Branch != null ? new BranchDto
                {
                    Id = user.Branch.Id,
                    Code = user.Branch.Code,
                    Name = user.Branch.Name,
                    Address = user.Branch.Address,
                    ContactPhone = user.Branch.ContactPhone,
                    ContactEmail = user.Branch.ContactEmail,
                    IsActive = user.Branch.IsActive
                } : null,
                Profiles = user.Profiles.Select(p => new ProfileDto
                {
                    Id = p.Id,
                    DisplayName = p.DisplayName,
                    ProfileType = p.ProfileType.ToString()
                }).ToList(),
                SelectedProfileId = null, // TODO: Get from claim or session if needed
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };

            return response;
        }

    }
}
