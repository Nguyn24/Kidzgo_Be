using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Registrations.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Registrations.CreateRegistration;

public sealed class CreateRegistrationCommandHandler(
    IDbContext context
) : ICommandHandler<CreateRegistrationCommand, CreateRegistrationResponse>
{
    public async Task<Result<CreateRegistrationResponse>> Handle(
        CreateRegistrationCommand command,
        CancellationToken cancellationToken)
    {
        // 1. Validate Student Profile exists and is active
        var student = await context.Profiles
            .FirstOrDefaultAsync(p => p.Id == command.StudentProfileId && p.ProfileType == Kidzgo.Domain.Users.ProfileType.Student, cancellationToken);

        if (student == null)
        {
            return Result.Failure<CreateRegistrationResponse>(RegistrationErrors.StudentNotFound(command.StudentProfileId));
        }

        // 2. Validate Branch exists
        var branchExists = await context.Branches
            .AnyAsync(b => b.Id == command.BranchId && b.IsActive, cancellationToken);

        if (!branchExists)
        {
            return Result.Failure<CreateRegistrationResponse>(RegistrationErrors.BranchNotFound(command.BranchId));
        }

        // 3. Validate Program exists
        var program = await context.Programs
            .FirstOrDefaultAsync(p => p.Id == command.ProgramId && p.IsActive && !p.IsDeleted, cancellationToken);

        if (program == null)
        {
            return Result.Failure<CreateRegistrationResponse>(RegistrationErrors.ProgramNotFound(command.ProgramId));
        }

        // 4. Validate TuitionPlan exists and belongs to the program
        var tuitionPlan = await context.TuitionPlans
            .FirstOrDefaultAsync(tp => tp.Id == command.TuitionPlanId && tp.ProgramId == command.ProgramId && tp.IsActive, cancellationToken);

        if (tuitionPlan == null)
        {
            return Result.Failure<CreateRegistrationResponse>(RegistrationErrors.TuitionPlanNotFound(command.TuitionPlanId));
        }

        // 5. Check if student already has active registration for this program
        var existingRegistration = await context.Registrations
            .AnyAsync(r => r.StudentProfileId == command.StudentProfileId 
                && r.ProgramId == command.ProgramId 
                && r.Status != RegistrationStatus.Completed 
                && r.Status != RegistrationStatus.Cancelled,
                cancellationToken);

        if (existingRegistration)
        {
            return Result.Failure<CreateRegistrationResponse>(
                RegistrationErrors.AlreadyExists(command.StudentProfileId, command.ProgramId));
        }

        // 6. Create the registration
        var now = DateTime.UtcNow;
        var registration = new Registration
        {
            Id = Guid.NewGuid(),
            StudentProfileId = command.StudentProfileId,
            BranchId = command.BranchId,
            ProgramId = command.ProgramId,
            TuitionPlanId = command.TuitionPlanId,
            RegistrationDate = now,
            ExpectedStartDate = command.ExpectedStartDate,
            PreferredSchedule = command.PreferredSchedule,
            Note = command.Note,
            Status = RegistrationStatus.New,
            TotalSessions = tuitionPlan.TotalSessions,
            UsedSessions = 0,
            RemainingSessions = tuitionPlan.TotalSessions,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Registrations.Add(registration);
        await context.SaveChangesAsync(cancellationToken);

        // 7. Return response
        return new CreateRegistrationResponse
        {
            Id = registration.Id,
            StudentProfileId = registration.StudentProfileId,
            BranchId = registration.BranchId,
            ProgramId = registration.ProgramId,
            ProgramName = program.Name,
            TuitionPlanId = registration.TuitionPlanId,
            TuitionPlanName = tuitionPlan.Name,
            RegistrationDate = registration.RegistrationDate,
            ExpectedStartDate = registration.ExpectedStartDate,
            PreferredSchedule = registration.PreferredSchedule,
            Note = registration.Note,
            Status = registration.Status.ToString(),
            ClassId = null,
            ClassName = null,
            CreatedAt = registration.CreatedAt
        };
    }
}
