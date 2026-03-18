namespace Kidzgo.Domain.Registrations;

public enum RegistrationStatus
{
    New,              // Mới tạo
    WaitingForClass, // Chờ xếp lớp
    ClassAssigned,   // Đã xếp lớp (chưa bắt đầu học)
    Studying,         // Đang học
    Paused,          // Bảo lưu
    Completed,        // Hoàn thành
    Cancelled         // Hủy
}
