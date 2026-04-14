using Kidzgo.Domain.Tickets;
using Kidzgo.Domain.Users;

namespace Kidzgo.Application.IncidentReports.Shared;

internal static class IncidentReportAccessPolicy
{
    public static bool CanUseIncidentFlow(UserRole role) =>
        role is UserRole.Admin or UserRole.ManagementStaff or UserRole.AccountantStaff or UserRole.Teacher;

    public static bool CanViewAll(UserRole role) => role == UserRole.Admin;

    public static bool CanView(User user, Ticket ticket) =>
        CanViewAll(user.Role) || ticket.OpenedByUserId == user.Id;

    public static bool CanComment(User user, Ticket ticket) =>
        CanViewAll(user.Role) || ticket.OpenedByUserId == user.Id;

    public static bool CanAssign(UserRole role) => role == UserRole.Admin;

    public static bool CanUpdateStatus(UserRole role) => role == UserRole.Admin;

    public static bool IsValidAssigneeRole(UserRole role) =>
        role is UserRole.Admin or UserRole.ManagementStaff or UserRole.AccountantStaff or UserRole.Teacher;
}
