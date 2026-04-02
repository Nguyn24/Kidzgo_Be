using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FieldsProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginAt",
                schema: "public",
                table: "Profiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSeenAt",
                schema: "public",
                table: "Profiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "public",
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
                column: "Body",
                value: "<p>Xin chào {{user_name}},</p>\n<p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>\n<p>Vui lòng bấm vào đường dẫn sau để đặt lại mật khẩu:</p>\n<p><a href=\"{{reset_link}}\">Đặt lại mật khẩu</a></p>\n<p>Nếu bạn không thực hiện yêu cầu này, hãy bỏ qua email.</p>\n<p>Trân trọng,<br/>Rex English Team</p>");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: new Guid("b9f6c8a1-3f57-45c6-8f4b-9f0c2b7d7f10"),
                column: "Body",
                value: "<div style=\"margin:0;padding:0;background:#f4f7fb;font-family:Segoe UI,Roboto,Arial,sans-serif;color:#1f2937;\">\n  <table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"background:#f4f7fb;padding:24px 12px;\">\n    <tr>\n      <td align=\"center\">\n        <table role=\"presentation\" width=\"640\" cellspacing=\"0\" cellpadding=\"0\" style=\"max-width:640px;background:#ffffff;border-radius:18px;overflow:hidden;box-shadow:0 10px 30px rgba(15,23,42,.08);\">\n          <tr>\n            <td style=\"padding:0;background:linear-gradient(135deg,#0ea5e9 0%,#2563eb 100%);\">\n              <div style=\"padding:28px 30px 24px 30px;color:#ffffff;\">\n                <p style=\"margin:0 0 8px 0;font-size:13px;letter-spacing:.08em;text-transform:uppercase;opacity:.9;\">KidzGo Learning Center</p>\n                <h1 style=\"margin:0;font-size:28px;line-height:1.3;font-weight:700;\">Hồ sơ mới đã sẵn sàng</h1>\n                <p style=\"margin:10px 0 0 0;font-size:15px;line-height:1.6;opacity:.95;\">\n                  Xin chào {{recipient_name}}, tài khoản của bạn hiện có {{profile_count}} hồ sơ đã được phê duyệt và sẵn sàng cho bước xác minh.\n                </p>\n              </div>\n            </td>\n          </tr>\n          <tr>\n            <td style=\"padding:26px 30px 12px 30px;\">\n              <p style=\"margin:0 0 14px 0;font-size:14px;line-height:1.7;color:#475569;\">\n                Vui lòng kiểm tra thông tin bên dưới. Mật khẩu đăng nhập và mã PIN phụ huynh hiện đang là mặc định, vui lòng đổi lại sau khi đăng nhập.\n              </p>\n            </td>\n          </tr>\n          <tr>\n            <td style=\"padding:0 30px 20px 30px;\">\n              <table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"border:1px solid #dbeafe;border-radius:12px;background:#eff6ff;\">\n                <tr>\n                  <td style=\"padding:16px 18px;\">\n                    <p style=\"margin:0 0 10px 0;font-size:13px;color:#1d4ed8;text-transform:uppercase;letter-spacing:.04em;\">Thông tin tài khoản</p>\n                    <p style=\"margin:0 0 6px 0;font-size:14px;\"><strong>Email đăng nhập:</strong> {{email}}</p>\n                    <p style=\"margin:0 0 6px 0;font-size:14px;\"><strong>Số điện thoại:</strong> {{phone}}</p>\n                    <p style=\"margin:0 0 6px 0;font-size:14px;\"><strong>Mật khẩu mặc định:</strong> {{password}}</p>\n                    <p style=\"margin:0;font-size:14px;\"><strong>PIN phụ huynh mặc định:</strong> {{pin}}</p>\n                  </td>\n                </tr>\n              </table>\n            </td>\n          </tr>\n          <tr>\n            <td style=\"padding:0 30px 8px 30px;\">\n              <p style=\"margin:0 0 12px 0;font-size:13px;color:#64748b;\">Danh sách hồ sơ đã được duyệt</p>\n              {{profiles_html}}\n            </td>\n          </tr>\n          <tr>\n            <td style=\"padding:8px 30px 28px 30px;\">\n              <a href=\"{{verify_link}}\" style=\"display:inline-block;background:#2563eb;color:#ffffff;text-decoration:none;font-weight:600;font-size:14px;padding:12px 18px;border-radius:10px;\">Xác minh tất cả hồ sơ</a>\n              <p style=\"margin:14px 0 0 0;font-size:12px;line-height:1.6;color:#64748b;\">\n                Nút xác minh sẽ kích hoạt toàn bộ hồ sơ đã được duyệt của tài khoản này.\n              </p>\n            </td>\n          </tr>\n          <tr>\n            <td style=\"padding:18px 30px;background:#f8fafc;border-top:1px solid #e2e8f0;\">\n              <p style=\"margin:0;font-size:12px;line-height:1.7;color:#64748b;\">\n                Nếu bạn không thực hiện thao tác này, vui lòng bỏ qua email hoặc liên hệ bộ phận hỗ trợ của KidzGo.\n              </p>\n            </td>\n          </tr>\n        </table>\n      </td>\n    </tr>\n  </table>\n</div>");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                column: "Content",
                value: "<div style=\"font-family: Arial, sans-serif; color:#222; line-height:1.6;\">\n  <h2 style=\"color:#2b6cb0;\">Yêu cầu bảo lưu đã được duyệt</h2>\n  <p>Xin chào,</p>\n  <p>Yêu cầu bảo lưu của <strong>{{student_name}}</strong> đã được duyệt.</p>\n  <div style=\"background:#f7fafc;border:1px solid #e2e8f0;border-radius:8px;padding:12px;\">\n    <p><strong>Thời gian bảo lưu:</strong> {{pause_from}} - {{pause_to}}</p>\n  </div>\n  <p>Vui lòng theo dõi lịch học sau thời gian bảo lưu.</p>\n  <p>Trân trọng,<br/>KidzGo Team</p>\n</div>");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                column: "Content",
                value: "<div style=\"font-family: Arial, sans-serif; color:#222; line-height:1.6;\">\n  <h2 style=\"color:#c53030;\">Yêu cầu bảo lưu bị từ chối</h2>\n  <p>Xin chào,</p>\n  <p>Yêu cầu bảo lưu của <strong>{{student_name}}</strong> đã bị từ chối.</p>\n  <div style=\"background:#fff5f5;border:1px solid #fed7d7;border-radius:8px;padding:12px;\">\n    <p><strong>Thời gian bảo lưu:</strong> {{pause_from}} - {{pause_to}}</p>\n  </div>\n  <p>Vui lòng liên hệ trung tâm nếu cần hỗ trợ thêm.</p>\n  <p>Trân trọng,<br/>KidzGo Team</p>\n</div>");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "Content",
                value: "<div style=\"font-family: Arial, sans-serif; color:#222; line-height:1.6;\">\n  <h2 style=\"color:#2f855a;\">Kết quả bảo lưu đã được cập nhật</h2>\n  <p>Xin chào,</p>\n  <p>Kết quả bảo lưu của <strong>{{student_name}}</strong> đã được cập nhật.</p>\n  <div style=\"background:#f0fff4;border:1px solid #c6f6d5;border-radius:8px;padding:12px;\">\n    <p><strong>Kết quả:</strong> {{outcome}}</p>\n    <p><strong>Ghi chú:</strong> {{outcome_note}}</p>\n  </div>\n  <p>Trân trọng,<br/>KidzGo Team</p>\n</div>");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastLoginAt",
                schema: "public",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "LastSeenAt",
                schema: "public",
                table: "Profiles");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
                column: "Body",
                value: "<p>Xin chào {{user_name}},</p>\r\n<p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>\r\n<p>Vui lòng bấm vào đường dẫn sau để đặt lại mật khẩu:</p>\r\n<p><a href=\"{{reset_link}}\">Đặt lại mật khẩu</a></p>\r\n<p>Nếu bạn không thực hiện yêu cầu này, hãy bỏ qua email.</p>\r\n<p>Trân trọng,<br/>Rex English Team</p>");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: new Guid("b9f6c8a1-3f57-45c6-8f4b-9f0c2b7d7f10"),
                column: "Body",
                value: "<div style=\"margin:0;padding:0;background:#f4f7fb;font-family:Segoe UI,Roboto,Arial,sans-serif;color:#1f2937;\">\r\n  <table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"background:#f4f7fb;padding:24px 12px;\">\r\n    <tr>\r\n      <td align=\"center\">\r\n        <table role=\"presentation\" width=\"640\" cellspacing=\"0\" cellpadding=\"0\" style=\"max-width:640px;background:#ffffff;border-radius:18px;overflow:hidden;box-shadow:0 10px 30px rgba(15,23,42,.08);\">\r\n          <tr>\r\n            <td style=\"padding:0;background:linear-gradient(135deg,#0ea5e9 0%,#2563eb 100%);\">\r\n              <div style=\"padding:28px 30px 24px 30px;color:#ffffff;\">\r\n                <p style=\"margin:0 0 8px 0;font-size:13px;letter-spacing:.08em;text-transform:uppercase;opacity:.9;\">KidzGo Learning Center</p>\r\n                <h1 style=\"margin:0;font-size:28px;line-height:1.3;font-weight:700;\">Hồ sơ mới đã sẵn sàng</h1>\r\n                <p style=\"margin:10px 0 0 0;font-size:15px;line-height:1.6;opacity:.95;\">\r\n                  Xin chào {{recipient_name}}, tài khoản của bạn hiện có {{profile_count}} hồ sơ đã được phê duyệt và sẵn sàng cho bước xác minh.\r\n                </p>\r\n              </div>\r\n            </td>\r\n          </tr>\r\n          <tr>\r\n            <td style=\"padding:26px 30px 12px 30px;\">\r\n              <p style=\"margin:0 0 14px 0;font-size:14px;line-height:1.7;color:#475569;\">\r\n                Vui lòng kiểm tra thông tin bên dưới. Mật khẩu đăng nhập và mã PIN phụ huynh hiện đang là mặc định, vui lòng đổi lại sau khi đăng nhập.\r\n              </p>\r\n            </td>\r\n          </tr>\r\n          <tr>\r\n            <td style=\"padding:0 30px 20px 30px;\">\r\n              <table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"border:1px solid #dbeafe;border-radius:12px;background:#eff6ff;\">\r\n                <tr>\r\n                  <td style=\"padding:16px 18px;\">\r\n                    <p style=\"margin:0 0 10px 0;font-size:13px;color:#1d4ed8;text-transform:uppercase;letter-spacing:.04em;\">Thông tin tài khoản</p>\r\n                    <p style=\"margin:0 0 6px 0;font-size:14px;\"><strong>Email đăng nhập:</strong> {{email}}</p>\r\n                    <p style=\"margin:0 0 6px 0;font-size:14px;\"><strong>Số điện thoại:</strong> {{phone}}</p>\r\n                    <p style=\"margin:0 0 6px 0;font-size:14px;\"><strong>Mật khẩu mặc định:</strong> {{password}}</p>\r\n                    <p style=\"margin:0;font-size:14px;\"><strong>PIN phụ huynh mặc định:</strong> {{pin}}</p>\r\n                  </td>\r\n                </tr>\r\n              </table>\r\n            </td>\r\n          </tr>\r\n          <tr>\r\n            <td style=\"padding:0 30px 8px 30px;\">\r\n              <p style=\"margin:0 0 12px 0;font-size:13px;color:#64748b;\">Danh sách hồ sơ đã được duyệt</p>\r\n              {{profiles_html}}\r\n            </td>\r\n          </tr>\r\n          <tr>\r\n            <td style=\"padding:8px 30px 28px 30px;\">\r\n              <a href=\"{{verify_link}}\" style=\"display:inline-block;background:#2563eb;color:#ffffff;text-decoration:none;font-weight:600;font-size:14px;padding:12px 18px;border-radius:10px;\">Xác minh tất cả hồ sơ</a>\r\n              <p style=\"margin:14px 0 0 0;font-size:12px;line-height:1.6;color:#64748b;\">\r\n                Nút xác minh sẽ kích hoạt toàn bộ hồ sơ đã được duyệt của tài khoản này.\r\n              </p>\r\n            </td>\r\n          </tr>\r\n          <tr>\r\n            <td style=\"padding:18px 30px;background:#f8fafc;border-top:1px solid #e2e8f0;\">\r\n              <p style=\"margin:0;font-size:12px;line-height:1.7;color:#64748b;\">\r\n                Nếu bạn không thực hiện thao tác này, vui lòng bỏ qua email hoặc liên hệ bộ phận hỗ trợ của KidzGo.\r\n              </p>\r\n            </td>\r\n          </tr>\r\n        </table>\r\n      </td>\r\n    </tr>\r\n  </table>\r\n</div>");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                column: "Content",
                value: "<div style=\"font-family: Arial, sans-serif; color:#222; line-height:1.6;\">\r\n  <h2 style=\"color:#2b6cb0;\">Yêu cầu bảo lưu đã được duyệt</h2>\r\n  <p>Xin chào,</p>\r\n  <p>Yêu cầu bảo lưu của <strong>{{student_name}}</strong> đã được duyệt.</p>\r\n  <div style=\"background:#f7fafc;border:1px solid #e2e8f0;border-radius:8px;padding:12px;\">\r\n    <p><strong>Thời gian bảo lưu:</strong> {{pause_from}} - {{pause_to}}</p>\r\n  </div>\r\n  <p>Vui lòng theo dõi lịch học sau thời gian bảo lưu.</p>\r\n  <p>Trân trọng,<br/>KidzGo Team</p>\r\n</div>");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                column: "Content",
                value: "<div style=\"font-family: Arial, sans-serif; color:#222; line-height:1.6;\">\r\n  <h2 style=\"color:#c53030;\">Yêu cầu bảo lưu bị từ chối</h2>\r\n  <p>Xin chào,</p>\r\n  <p>Yêu cầu bảo lưu của <strong>{{student_name}}</strong> đã bị từ chối.</p>\r\n  <div style=\"background:#fff5f5;border:1px solid #fed7d7;border-radius:8px;padding:12px;\">\r\n    <p><strong>Thời gian bảo lưu:</strong> {{pause_from}} - {{pause_to}}</p>\r\n  </div>\r\n  <p>Vui lòng liên hệ trung tâm nếu cần hỗ trợ thêm.</p>\r\n  <p>Trân trọng,<br/>KidzGo Team</p>\r\n</div>");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "Content",
                value: "<div style=\"font-family: Arial, sans-serif; color:#222; line-height:1.6;\">\r\n  <h2 style=\"color:#2f855a;\">Kết quả bảo lưu đã được cập nhật</h2>\r\n  <p>Xin chào,</p>\r\n  <p>Kết quả bảo lưu của <strong>{{student_name}}</strong> đã được cập nhật.</p>\r\n  <div style=\"background:#f0fff4;border:1px solid #c6f6d5;border-radius:8px;padding:12px;\">\r\n    <p><strong>Kết quả:</strong> {{outcome}}</p>\r\n    <p><strong>Ghi chú:</strong> {{outcome_note}}</p>\r\n  </div>\r\n  <p>Trân trọng,<br/>KidzGo Team</p>\r\n</div>");
        }
    }
}
