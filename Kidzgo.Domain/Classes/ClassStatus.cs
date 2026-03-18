namespace Kidzgo.Domain.Classes;

public enum ClassStatus
{
    Planned,         // Sắp khai giảng / Đã lên kế hoạch
    Recruiting,      // Đang tuyển sinh
    Active,         // Đang học
    Full,           // Đã đầy (vẫn có thể vào nếu có người nghỉ)
    Closed,         // Đã đóng / Kết thúc (alias cho Completed)
    Completed,      // Kết thúc / Hoàn thành
    Suspended,      // Tạm ngưng
    Cancelled     // Hủy
}
