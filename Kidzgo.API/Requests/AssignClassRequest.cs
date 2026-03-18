namespace Kidzgo.API.Requests;

public sealed class AssignClassRequest
{
    /// <summary>
    /// Class ID to assign. Required for immediate/makeup, optional for wait.
    /// </summary>
    public Guid? ClassId { get; set; }
    
    /// <summary>
    /// Entry type: "immediate" | "makeup" | "wait"
    /// - immediate: Vào học ngay, tham gia các buổi còn lại
    /// - makeup: Đã có lớp nhưng cần học bổ trước khi vào lớp
    /// - wait: Chờ lớp mới, chưa xếp lớp
    /// </summary>
    public string EntryType { get; set; } = "immediate";
}
