# Deployment Checklist - Kidzgo Backend

## Trước khi deploy

- [ ] Đã có quyền truy cập Remote Desktop vào VPS
- [ ] Đã cài đặt Docker Desktop trên VPS
- [ ] Đã cài đặt Git trên VPS
- [ ] Đã clone repository về VPS

## Cấu hình

- [ ] Đã cập nhật `appsettings.Production.json` với thông tin đúng
- [ ] Đã đổi mật khẩu PostgreSQL trong `compose.yaml`
- [ ] Đã đảm bảo mật khẩu PostgreSQL khớp giữa `compose.yaml` và `appsettings.Production.json`
- [ ] Đã cập nhật JWT Secret (ít nhất 32 ký tự)
- [ ] Đã cập nhật ClientUrl trong appsettings
- [ ] Đã cập nhật tất cả API keys và secrets

## Deploy

- [ ] Đã chạy `docker-compose up -d --build`
- [ ] Đã kiểm tra containers đang chạy: `docker-compose ps`
- [ ] Đã kiểm tra logs không có lỗi: `docker-compose logs`

## Kiểm tra sau deploy

- [ ] Health check hoạt động: `http://YOUR_VPS_IP/health`
- [ ] Swagger UI có thể truy cập: `http://YOUR_VPS_IP/swagger`
- [ ] Database connection thành công (kiểm tra logs)
- [ ] Migrations đã được áp dụng (kiểm tra logs)
- [ ] Có thể gọi API endpoints từ bên ngoài

## Bảo mật

- [ ] Đã mở firewall cho port 80 (và 443 nếu cần)
- [ ] Đã đổi tất cả mật khẩu mặc định
- [ ] Đã cấu hình backup database
- [ ] Đã kiểm tra không expose port 5432 ra ngoài (nếu không cần)

## Monitoring

- [ ] Đã kiểm tra Seq logs: `http://YOUR_VPS_IP:8081`
- [ ] Đã thiết lập monitoring/alerting (nếu có)

