using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Users.Admin.UpdateUser;

public sealed class UpdateUserCommandHandler(IDbContext context, IUserContext userContext, ISender sender)
    : ICommandHandler<UpdateUserCommand, UpdateUserResponse>
{
    public async Task<Result<UpdateUserResponse>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null)
        {
            return Result.Failure<UpdateUserResponse>(UserErrors.NotFound(request.UserId));
        }
        
        // Parse and validate role if provided
        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
            {
                return Result.Failure<UpdateUserResponse>(UserErrors.InvalidRole(request.Role));
            }
            user.Role = role;
        }

        if (request.TeacherCompensationType != null)
        {
            if (string.IsNullOrWhiteSpace(request.TeacherCompensationType))
            {
                user.TeacherCompensationType = null;
            }
            else if (Enum.TryParse<TeacherCompensationType>(request.TeacherCompensationType, true, out var teacherCompensationType))
            {
                user.TeacherCompensationType = teacherCompensationType;
            }
        }

        user.Username = request.Username ?? user.Username;
        user.Name = request.Name ?? user.Name;
        user.Email = request.Email ?? user.Email;
        if (request.PhoneNumber != null)
        {
            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                user.PhoneNumber = null;
            }
            else
            {
                var phoneLookupCandidates = PhoneNumberNormalizer.GetLookupCandidates(request.PhoneNumber);

                var phoneNumberExists = await context.Users.AnyAsync(
                    u => u.Id != request.UserId &&
                         u.PhoneNumber != null &&
                         phoneLookupCandidates.Contains(
                             u.PhoneNumber
                                 .Replace(" ", "")
                                 .Replace("-", "")
                                 .Replace(".", "")
                                 .Replace("(", "")
                                 .Replace(")", "")
                                 .Replace("+", "")),
                    cancellationToken);

                if (phoneNumberExists)
                {
                    return Result.Failure<UpdateUserResponse>(UserErrors.PhoneNumberNotUnique);
                }

                user.PhoneNumber = PhoneNumberNormalizer.NormalizeVietnamesePhoneNumber(request.PhoneNumber);
            }
        }
        user.IsActive = request.IsActive ?? user.IsActive;
        user.IsDeleted = request.isDeleted ?? user.IsDeleted;
        if (user.Role != UserRole.Teacher)
        {
            user.TeacherCompensationType = null;
        }
        user.UpdatedAt = VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new UpdateUserResponse(user));
    }
}
