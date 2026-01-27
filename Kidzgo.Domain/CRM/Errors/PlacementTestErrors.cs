using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.CRM.Errors;

public static class PlacementTestErrors
{
    public static Error NotFound(Guid? placementTestId) => Error.NotFound(
        "PlacementTest.NotFound",
        $"The placement test with the Id = '{placementTestId}' was not found");

    public static Error LeadNotFound(Guid? leadId) => Error.NotFound(
        "PlacementTest.LeadNotFound",
        $"The lead with the Id = '{leadId}' was not found");

    public static Error StudentProfileNotFound(Guid? profileId) => Error.NotFound(
        "PlacementTest.StudentProfileNotFound",
        $"The student profile with the Id = '{profileId}' was not found");

    public static Error ClassNotFound(Guid? classId) => Error.NotFound(
        "PlacementTest.ClassNotFound",
        $"The class with the Id = '{classId}' was not found");

    public static Error InvigilatorNotFound(Guid? userId) => Error.NotFound(
        "PlacementTest.InvigilatorNotFound",
        $"The invigilator user with the Id = '{userId}' was not found");

    public static readonly Error InvalidStatusTransition = Error.Validation(
        "PlacementTest.InvalidStatusTransition",
        "Invalid status transition. Cannot change from current status to target status");

    public static readonly Error CannotUpdateCompletedTest = Error.Validation(
        "PlacementTest.CannotUpdateCompletedTest",
        "Cannot update a placement test that has been completed");

    public static readonly Error CannotCancelCompletedTest = Error.Validation(
        "PlacementTest.CannotCancelCompletedTest",
        "Cannot cancel a placement test that has been completed");

    public static readonly Error CannotMarkNoShowCompletedTest = Error.Validation(
        "PlacementTest.CannotMarkNoShowCompletedTest",
        "Cannot mark a completed placement test as NoShow");

    public static readonly Error LeadAlreadyEnrolled = Error.Conflict(
        "PlacementTest.LeadAlreadyEnrolled",
        "The lead has already been converted to enrollment");
}

