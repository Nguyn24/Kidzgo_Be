using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.CRM.Errors;

public static class LeadErrors
{
    public static Error NotFound(Guid? leadId) => Error.NotFound(
        "Lead.NotFound",
        $"The lead with the Id = '{leadId}' was not found");

    public static readonly Error InvalidContactInfo = Error.Validation(
        "Lead.InvalidContactInfo",
        "At least one of ContactName, Phone, or Email must be provided");

    public static readonly Error InvalidSource = Error.Validation(
        "Lead.InvalidSource",
        "Invalid lead source. Valid values: Landing, Zalo, Referral, Offline");

    public static readonly Error InvalidStatus = Error.Validation(
        "Lead.InvalidStatus",
        "Invalid lead status. Valid values: New, Contacted, BookedTest, TestDone, Enrolled, Lost");

    public static Error OwnerNotFound(Guid? ownerId) => Error.NotFound(
        "Lead.OwnerNotFound",
        $"The staff user with the Id = '{ownerId}' was not found");

    public static readonly Error OwnerNotStaff = Error.Validation(
        "Lead.OwnerNotStaff",
        "The assigned owner must be a Staff user (ManagementStaff or AccountantStaff)");

    public static readonly Error BranchNotFound = Error.NotFound(
        "Lead.BranchNotFound",
        "The specified branch was not found");

    public static readonly Error DuplicateLead = Error.Conflict(
        "Lead.DuplicateLead",
        "A lead with the same phone, email, or Zalo ID already exists");

    public static readonly Error CannotUpdateConvertedLead = Error.Validation(
        "Lead.CannotUpdateConvertedLead",
        "Cannot update a lead that has been converted to enrollment");

    public static readonly Error InvalidStatusTransition = Error.Validation(
        "Lead.InvalidStatusTransition",
        "Invalid status transition. Cannot change from current status to target status");
}

