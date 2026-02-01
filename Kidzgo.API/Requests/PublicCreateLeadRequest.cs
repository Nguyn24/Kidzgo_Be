using Kidzgo.Domain.CRM;

namespace Kidzgo.API.Requests;

/// <summary>
/// Request body đơn giản cho form web public tạo lead.
/// Chỉ giữ 4 field cơ bản, các field khác sẽ được backend set mặc định/null.
/// </summary>
public sealed class PublicCreateLeadRequest
{
    public string ContactName { get; set; } = null!;
    public string? Phone { get; set; }
    public string? ZaloId { get; set; }
    public string? Email { get; set; }
}


