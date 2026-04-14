using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Tickets.Errors;

public static class IncidentReportErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        "IncidentReport.NotFound",
        $"Incident report with Id = '{id}' was not found");

    public static readonly Error Unauthorized = Error.Unauthorized(
        "IncidentReport.Unauthorized",
        "You do not have permission to access this incident report");

    public static readonly Error InvalidRole = Error.Unauthorized(
        "IncidentReport.InvalidRole",
        "Your role cannot use incident reports");

    public static readonly Error IncidentOnly = Error.Validation(
        "IncidentReport.IncidentOnly",
        "This ticket is not an incident report");

    public static readonly Error AssignAdminOnly = Error.Unauthorized(
        "IncidentReport.AssignAdminOnly",
        "Only admins can assign incident reports");

    public static readonly Error UpdateStatusAdminOnly = Error.Unauthorized(
        "IncidentReport.UpdateStatusAdminOnly",
        "Only admins can update incident report status");

    public static readonly Error AssignedUserNotFound = Error.NotFound(
        "IncidentReport.AssignedUserNotFound",
        "Assigned handler not found");

    public static readonly Error AssignedUserBranchMismatch = Error.Conflict(
        "IncidentReport.AssignedUserBranchMismatch",
        "Assigned handler must belong to the same branch as the incident");

    public static Error InvalidStatusTransition(string current, string target) => Error.Conflict(
        "IncidentReport.InvalidStatusTransition",
        $"Cannot change incident status from {current} to {target}");
}
