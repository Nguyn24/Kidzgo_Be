using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Registrations.AssignClass;

public sealed class AssignClassCommand : ICommand<AssignClassResponse>
{
    public Guid RegistrationId { get; init; }
    
    /// <summary>
    /// Class ID to assign. Required for immediate/makeup, optional for wait.
    /// </summary>
    public Guid? ClassId { get; init; }
    
    /// <summary>
    /// Entry type: "immediate" | "makeup" | "wait"
    /// - immediate: Vào học ngay, tham gia các buổi còn lại
    /// - makeup: Đã có lớp nhưng cần học bổ trước khi vào lớp
    /// - wait: Chờ lớp mới, chưa xếp lớp
    /// </summary>
    public string EntryType { get; init; } = "immediate";

    /// <summary>
    /// Track to assign: "primary" | "secondary"
    /// </summary>
    public string Track { get; init; } = "primary";

    /// <summary>
    /// Optional RRULE subset of the class schedule pattern for this student.
    /// </summary>
    public string? SessionSelectionPattern { get; init; }
}
