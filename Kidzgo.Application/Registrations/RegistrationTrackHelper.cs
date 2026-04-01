using Kidzgo.Domain.Registrations;

namespace Kidzgo.Application.Registrations;

internal static class RegistrationTrackHelper
{
    internal const string PrimaryTrack = "primary";
    internal const string SecondaryTrack = "secondary";

    internal static string NormalizeTrack(string? track)
    {
        return string.Equals(track, SecondaryTrack, StringComparison.OrdinalIgnoreCase)
            ? SecondaryTrack
            : PrimaryTrack;
    }

    internal static EntryType ParseEntryType(string? entryType)
    {
        return entryType?.ToLowerInvariant() switch
        {
            "makeup" => EntryType.Makeup,
            "wait" => EntryType.Wait,
            "retake" => EntryType.Retake,
            _ => EntryType.Immediate
        };
    }

    internal static RegistrationStatus ResolveStatus(Registration registration)
    {
        var hasImmediateTrack =
            HasAssignedTrack(registration.ClassId, registration.EntryType, EntryType.Immediate) ||
            HasAssignedTrack(registration.SecondaryClassId, registration.SecondaryEntryType, EntryType.Immediate);

        if (hasImmediateTrack)
        {
            return RegistrationStatus.Studying;
        }

        var hasAssignedTrack =
            HasAssignedTrack(registration.ClassId, registration.EntryType, EntryType.Makeup) ||
            HasAssignedTrack(registration.SecondaryClassId, registration.SecondaryEntryType, EntryType.Makeup) ||
            HasAssignedTrack(registration.ClassId, registration.EntryType, EntryType.Retake) ||
            HasAssignedTrack(registration.SecondaryClassId, registration.SecondaryEntryType, EntryType.Retake);

        if (hasAssignedTrack)
        {
            return RegistrationStatus.ClassAssigned;
        }

        if (registration.ClassId == null ||
            (registration.SecondaryProgramId.HasValue && registration.SecondaryClassId == null))
        {
            return RegistrationStatus.WaitingForClass;
        }

        return RegistrationStatus.New;
    }

    private static bool HasAssignedTrack(Guid? classId, EntryType? currentEntryType, EntryType expectedEntryType)
    {
        return classId.HasValue && currentEntryType == expectedEntryType;
    }
}
