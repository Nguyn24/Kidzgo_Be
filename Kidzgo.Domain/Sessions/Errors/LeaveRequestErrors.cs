using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Sessions.Errors;

public static class LeaveRequestErrors
{
    public static Error NotFound(Guid? id) => Error.NotFound(
        "LeaveRequest.NotFound",
        $"The leave request with Id = '{id}' was not found.");

    public static readonly Error AlreadyApproved = Error.Conflict(
        "LeaveRequest.AlreadyApproved",
        "Leave request is already approved.");

    public static readonly Error AlreadyRejected = Error.Conflict(
        "LeaveRequest.AlreadyRejected",
        "Leave request is already rejected.");

    public static Error NotEnrolled(Guid classId, Guid studentProfileId) => Error.Validation(
        "LeaveRequest.NotEnrolled",
        $"Student profile '{studentProfileId}' is not enrolled in class '{classId}'.");

    public static Error SessionNotFound(Guid classId, DateOnly sessionDate) => Error.NotFound(
        "LeaveRequest.SessionNotFound",
        $"No session found for class '{classId}' on date '{sessionDate:yyyy-MM-dd}'.");
}

