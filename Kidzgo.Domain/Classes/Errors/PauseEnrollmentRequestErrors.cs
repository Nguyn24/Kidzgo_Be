using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Classes.Errors;

public static class PauseEnrollmentRequestErrors
{
    public static Error NotFound(Guid requestId) => Error.NotFound(
        "PauseEnrollmentRequest.NotFound",
        $"Pause enrollment request with Id = '{requestId}' was not found");

    public static Error StudentNotFound(Guid studentProfileId) => Error.NotFound(
        "PauseEnrollmentRequest.StudentNotFound",
        $"Student profile with Id = '{studentProfileId}' was not found");

    public static Error ClassNotFound(Guid classId) => Error.NotFound(
        "PauseEnrollmentRequest.ClassNotFound",
        $"Class with Id = '{classId}' was not found");

    public static Error NotEnrolled(Guid classId, Guid studentProfileId) => Error.Conflict(
        "PauseEnrollmentRequest.NotEnrolled",
        $"Student '{studentProfileId}' is not enrolled in class '{classId}'");

    public static readonly Error EnrollmentNotActive = Error.Conflict(
        "PauseEnrollmentRequest.EnrollmentNotActive",
        "Only active enrollments can be paused");

    public static readonly Error DuplicateActiveRequest = Error.Conflict(
        "PauseEnrollmentRequest.DuplicateActiveRequest",
        "A pending or approved pause request already exists for this student in the selected date range");

    public static readonly Error NoEnrollmentsInRange = Error.Conflict(
        "PauseEnrollmentRequest.NoEnrollmentsInRange",
        "Student has no active enrollments with sessions in the pause range");

    public static readonly Error AlreadyApproved = Error.Conflict(
        "PauseEnrollmentRequest.AlreadyApproved",
        "Pause enrollment request is already approved");

    public static readonly Error AlreadyRejected = Error.Conflict(
        "PauseEnrollmentRequest.AlreadyRejected",
        "Pause enrollment request is already rejected");

    public static readonly Error AlreadyCancelled = Error.Conflict(
        "PauseEnrollmentRequest.AlreadyCancelled",
        "Pause enrollment request is already cancelled");

    public static Error CancelWindowExpired(DateOnly pauseFrom) => Error.Conflict(
        "PauseEnrollmentRequest.CancelWindowExpired",
        $"Cannot cancel after pause start date '{pauseFrom}'");

    public static readonly Error OutcomeNotAllowed = Error.Conflict(
        "PauseEnrollmentRequest.OutcomeNotAllowed",
        "Outcome can only be set for approved requests");
}
