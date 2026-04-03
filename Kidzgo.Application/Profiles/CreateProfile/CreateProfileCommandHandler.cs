using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.CRM;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using Kidzgo.Domain.Users.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Profiles.CreateProfile;

public sealed class CreateProfileCommandHandler(
    IDbContext context,
    IPasswordHasher passwordHasher,
    IMediator mediator
) : ICommandHandler<CreateProfileCommand, CreateProfileResponse>
{
    public async Task<Result<CreateProfileResponse>> Handle(CreateProfileCommand command, CancellationToken cancellationToken)
    {
        // Check if user exists
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<CreateProfileResponse>(ProfileErrors.UserNotFound);
        }

        var now = DateTime.UtcNow;
        var trimmedDisplayName = command.DisplayName.Trim();
        var trimmedFullName = string.IsNullOrWhiteSpace(command.FullName)
            ? null
            : command.FullName.Trim();
        var lookupName = trimmedFullName ?? trimmedDisplayName;
        
        var profile = new Profile
        {
            Id = Guid.NewGuid(),
            UserId = command.UserId,
            ProfileType = command.ProfileType,
            DisplayName = trimmedDisplayName,
            Name = trimmedFullName,
            PinHash = !string.IsNullOrWhiteSpace(command.PinHash) 
                ? passwordHasher.Hash(command.PinHash) 
                : null,
            IsApproved = false,
            IsActive = false,
            IsDeleted = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        // Auto-fill student data from LeadChild if creating Student profile
        if (command.ProfileType == ProfileType.Student)
        {
            var leadChild = await context.LeadChildren
                .Include(lc => lc.Lead)
                .Where(lc => lc.Lead.Email == user.Email && lc.ChildName.Contains(lookupName))
                .FirstOrDefaultAsync(cancellationToken);

            if (leadChild != null)
            {
                profile.Name ??= leadChild.ChildName;
                profile.Gender = leadChild.Gender;
                profile.DateOfBirth = leadChild.Dob;
            }
        }

        if (command.ProfileType == ProfileType.Parent)
        {
            var lead = await context.Leads
                .Where(l=>l.Email == user.Email || l.Phone == user.PhoneNumber && l.ContactName.Contains(lookupName))
                .FirstOrDefaultAsync(cancellationToken);
            
            if (lead != null)
            {
                profile.Name ??= lead.ContactName;
                profile.ZaloId = lead.ZaloId;
            }
        }

        context.Profiles.Add(profile);

        // Automatically link profiles with the same UserId
        // If creating a Parent profile, link with existing Student profiles
        // If creating a Student profile, link with existing Parent profiles
        if (command.ProfileType == ProfileType.Parent)
        {
            var existingStudentProfiles = await context.Profiles
                .Where(p => p.UserId == command.UserId && 
                           p.ProfileType == ProfileType.Student && 
                           !p.IsDeleted)
                .ToListAsync(cancellationToken);

            foreach (var studentProfile in existingStudentProfiles)
            {
               
                var link = new ParentStudentLink
                {
                    Id = Guid.NewGuid(),
                    ParentProfileId = profile.Id,
                    StudentProfileId = studentProfile.Id,
                    CreatedAt = now
                };
                context.ParentStudentLinks.Add(link);
            }
        }
        else if (command.ProfileType == ProfileType.Student)
        {
            var existingParentProfiles = await context.Profiles
                .Where(p => p.UserId == command.UserId && 
                           p.ProfileType == ProfileType.Parent && 
                           !p.IsDeleted)
                .ToListAsync(cancellationToken);

            foreach (var parentProfile in existingParentProfiles)
            {
                
                var link = new ParentStudentLink
                {
                    Id = Guid.NewGuid(),
                    ParentProfileId = parentProfile.Id,
                    StudentProfileId = profile.Id,
                    CreatedAt = now
                };
                context.ParentStudentLinks.Add(link);
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        return new CreateProfileResponse
        {
            Id = profile.Id,
            UserId = profile.UserId,
            ProfileType = profile.ProfileType.ToString(),
            DisplayName = profile.DisplayName,
            Name = profile.Name,
            IsActive = profile.IsActive,
            CreatedAt = profile.CreatedAt,
            UpdatedAt = profile.UpdatedAt
        };
    }
}
