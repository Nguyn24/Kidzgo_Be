using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Classes.Errors;

public static class EnrollmentErrors
{
    public static Error NotFound(Guid? enrollmentId) => Error.NotFound(
        "Enrollment.NotFound",
        $"Enrollment with Id = '{enrollmentId}' was not found");

    public static readonly Error StudentNotFound = Error.NotFound(
        "Enrollment.StudentNotFound",
        "Student profile not found or is not a student");

    public static readonly Error ClassNotFound = Error.NotFound(
        "Enrollment.ClassNotFound",
        "Class not found");

    public static readonly Error ClassNotAvailable = Error.Conflict(
        "Enrollment.ClassNotAvailable",
        "Class is not available for enrollment");

    public static readonly Error AlreadyEnrolled = Error.Conflict(
        "Enrollment.AlreadyEnrolled",
        "Student is already enrolled in this class");

    public static readonly Error ClassFull = Error.Conflict(
        "Enrollment.ClassFull",
        "Class has reached its capacity");

    public static readonly Error TuitionPlanNotFound = Error.NotFound(
        "Enrollment.TuitionPlanNotFound",
        "Tuition plan not found");

    public static readonly Error TuitionPlanNotAvailable = Error.Conflict(
        "Enrollment.TuitionPlanNotAvailable",
        "Tuition plan is not available");

    public static readonly Error TuitionPlanProgramMismatch = Error.Conflict(
        "Enrollment.TuitionPlanProgramMismatch",
        "Tuition plan must belong to the same program as the class");

    public static readonly Error AlreadyActive = Error.Conflict(
        "Enrollment.AlreadyActive",
        "Enrollment is already active");

    public static readonly Error CannotReactivateDropped = Error.Conflict(
        "Enrollment.CannotReactivateDropped",
        "Cannot reactivate a dropped enrollment");

    public static readonly Error InvalidStatus = Error.Conflict(
        "Enrollment.InvalidStatus",
        "Only active enrollments can be paused");

    public static readonly Error AlreadyDropped = Error.Conflict(
        "Enrollment.AlreadyDropped",
        "Enrollment is already dropped");
}

