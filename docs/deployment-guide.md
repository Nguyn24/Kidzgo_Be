# Hướng dẫn Deploy Backend và Database lên VPS

## Yêu cầu hệ thống

- Windows Server với Remote Desktop đã được cấu hình
- Quyền Administrator
- Kết nối Internet ổn định

## Các bước chuẩn bị trên VPS

### Bước 1: Cài đặt Docker Desktop cho Windows

1. Tải Docker Desktop cho Windows từ: https://www.docker.com/products/docker-desktop/
2. Cài đặt và khởi động lại máy nếu cần
3. Mở Docker Desktop và đảm bảo Docker đang chạy

### Bước 2: Cài đặt Git (nếu chưa có)

1. Tải Git từ: https://git-scm.com/download/win
2. Cài đặt với các tùy chọn mặc định

### Bước 3: Clone project từ repository

Mở PowerShell hoặc Command Prompt và chạy:

```powershell
# Tạo thư mục cho project (ví dụ: C:\Projects)
mkdir C:\Projects
cd C:\Projects

# Clone repository (thay YOUR_REPO_URL bằng URL thực tế)
git clone YOUR_REPO_URL Kidzgo
cd Kidzgo
```

## Cấu hình Production

### Bước 4: Cập nhật file appsettings.Production.json

Chỉnh sửa file `Kidzgo.API/appsettings.Production.json` với các thông tin phù hợp:

```json
{
  "ConnectionStrings": {
    "Database": "Host=postgres;Port=5432;Database=kidzgo;Username=postgres;Password=YOUR_SECURE_PASSWORD;"
  },
  "ClientSettings": {
    "ClientUrl": "https://your-frontend-domain.com"
  },
  "Jwt": {
    "Secret": "YOUR_VERY_LONG_SECRET_KEY_HERE_AT_LEAST_32_CHARACTERS",
    "Issuer": "kidzgo-api",
    "Audience": "users",
    "ExpirationInMinutes": 60
  },
  "MailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password"
  },
  "Google": {
    "ClientId": "your-google-client-id"
  },
  "Cloudinary": {
    "CloudName": "your-cloudinary-name",
    "ApiKey": "your-api-key",
    "ApiSecret": "your-api-secret"
  },
  "PayOS": {
    "BaseUrl": "https://api.payos.vn",
    "ClientId": 0,
    "ApiKey": "your-payos-api-key",
    "ChecksumKey": "your-payos-checksum-key",
    "ReturnUrl": "https://your-frontend.com/invoices/{invoiceId}/success",
    "CancelUrl": "https://your-frontend.com/invoices/{invoiceId}/cancel"
  },
  "Quartz": {
    "Schedules": {
      "SyncPlannedToActualSessionsJob": "0 0/1 * * * ?",
      "AutoConfirmRewardRedemptionJob": "0 0 9 * * ?",
      "AutoConfirmRewardRedemptionJob_Days": 3,
      "MarkOverdueHomeworkSubmissionsJob": "0 0/5 * * * ?"
    }
  },
  "Zalo": {
    "AppId": "your-zalo-app-id",
    "AppSecret": "your-zalo-app-secret",
    "OAId": "your-zalo-oa-id",
    "WebhookVerifyToken": "your-webhook-token",
    "BaseUrl": "https://openapi.zalo.me/v2.0"
  }
}
```

**Lưu ý quan trọng:**
- Thay `YOUR_SECURE_PASSWORD` bằng mật khẩu mạnh cho PostgreSQL
- Cập nhật `ConnectionStrings.Database` để sử dụng `Host=postgres` (tên service trong Docker Compose)
- Đảm bảo JWT Secret đủ dài (ít nhất 32 ký tự)
- Cập nhật tất cả các thông tin API keys và secrets

### Bước 5: Cập nhật compose.yaml

Chỉnh sửa file `compose.yaml` ở thư mục gốc:

```yaml
services:
  kidzgo.api:
    image: kidzgo.api
    build:
      context: .
      dockerfile: Kidzgo.API/Dockerfile
    ports:
      - "80:8080"  # HTTP port
      - "443:8080"  # HTTPS port (nếu cần)
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
    depends_on:
      postgres:
        condition: service_healthy
    restart: unless-stopped
    networks:
      - kidzgo-network
  
  postgres:
    image: postgres:15
    container_name: postgres
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: kidzgo  # Thay đổi mật khẩu này!
      POSTGRES_DB: kidzgo
    ports:
      - "5432:5432"  # Chỉ expose nếu cần truy cập từ bên ngoài
    volumes:
      - pgdata:/var/lib/postgresql/data
    restart: unless-stopped
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 10s
      timeout: 5s
      retries: 5
    networks:
      - kidzgo-network

  seq:
    image: datalust/seq:2024.3
    container_name: seq
    environment:
      - ACCEPT_EULA=Y
    ports:
      - "8081:80"
    restart: unless-stopped
    networks:
      - kidzgo-network

volumes:
  pgdata:

networks:
  kidzgo-network:
    driver: bridge
```

**Quan trọng:** Đảm bảo mật khẩu PostgreSQL trong `compose.yaml` khớp với mật khẩu trong `appsettings.Production.json`.

## Deploy ứng dụng

### Bước 6: Build và khởi động containers

Mở PowerShell trong thư mục project và chạy:

```powershell
# Build và khởi động tất cả services
docker-compose up -d --build

# Kiểm tra trạng thái containers
docker-compose ps

# Xem logs của API
docker-compose logs -f kidzgo.api

# Xem logs của PostgreSQL
docker-compose logs -f postgres
```

### Bước 7: Kiểm tra ứng dụng

1. **Kiểm tra health check:**
   ```
   http://YOUR_VPS_IP/health
   ```

2. **Kiểm tra Swagger UI:**
   ```
   http://YOUR_VPS_IP/swagger
   ```

3. **Kiểm tra Seq (logging):**
   ```
   http://YOUR_VPS_IP:8081
   ```

## Cấu hình Firewall

### Bước 8: Mở ports cần thiết

1. Mở **Windows Defender Firewall with Advanced Security**
2. Tạo Inbound Rules cho các ports:
   - **Port 80** (HTTP) - Cho phép từ Any
   - **Port 443** (HTTPS) - Cho phép từ Any (nếu dùng HTTPS)
   - **Port 8081** (Seq) - Chỉ cho phép từ IP nội bộ (tùy chọn)

## Quản lý và bảo trì

### Xem logs

```powershell
# Xem logs của tất cả services
docker-compose logs -f

# Xem logs của một service cụ thể
docker-compose logs -f kidzgo.api
docker-compose logs -f postgres
```

### Dừng và khởi động lại

```powershell
# Dừng tất cả services
docker-compose stop

# Khởi động lại
docker-compose start

# Khởi động lại và rebuild
docker-compose up -d --build
```

### Backup database

```powershell
# Backup database
docker-compose exec postgres pg_dump -U postgres kidzgo > backup_$(Get-Date -Format "yyyyMMdd_HHmmss").sql

# Restore database
docker-compose exec -T postgres psql -U postgres kidzgo < backup_file.sql
```

### Cập nhật code mới

```powershell
# Pull code mới từ Git
git pull origin main

# Rebuild và restart
docker-compose up -d --build

# Kiểm tra logs để đảm bảo không có lỗi
docker-compose logs -f kidzgo.api
```

## Xử lý sự cố

### Container không khởi động

```powershell
# Kiểm tra logs chi tiết
docker-compose logs kidzgo.api

# Kiểm tra trạng thái container
docker ps -a

# Xóa và rebuild lại
docker-compose down
docker-compose up -d --build
```

### Database connection errors

1. Kiểm tra PostgreSQL đã khởi động:
   ```powershell
   docker-compose ps postgres
   ```

2. Kiểm tra connection string trong `appsettings.Production.json`
3. Đảm bảo mật khẩu khớp giữa `compose.yaml` và `appsettings.Production.json`

### Migration errors

Migrations sẽ tự động chạy khi API khởi động. Nếu có lỗi:

1. Kiểm tra logs:
   ```powershell
   docker-compose logs kidzgo.api | Select-String -Pattern "migration"
   ```

2. Nếu cần, chạy migration thủ công:
   ```powershell
   docker-compose exec kidzgo.api dotnet ef database update --project Kidzgo.Infrastructure --startup-project Kidzgo.API
   ```

## Cấu hình HTTPS (Tùy chọn)

Nếu muốn sử dụng HTTPS, bạn có thể:

1. Sử dụng reverse proxy như Nginx hoặc IIS
2. Hoặc cấu hình SSL certificate trực tiếp trong Docker

## Lưu ý bảo mật

1. **Đổi mật khẩu mặc định:** Đảm bảo đổi tất cả mật khẩu mặc định
2. **Environment variables:** Cân nhắc sử dụng environment variables thay vì hardcode trong file config
3. **Firewall:** Chỉ mở các ports cần thiết
4. **Backup:** Thiết lập backup database định kỳ
5. **Updates:** Thường xuyên cập nhật Docker images và dependencies

## Kiểm tra sau khi deploy

- [ ] API health check trả về OK
- [ ] Swagger UI có thể truy cập
- [ ] Database connection thành công
- [ ] Migrations đã được áp dụng
- [ ] Logs không có lỗi
- [ ] Có thể gọi API endpoints từ bên ngoài

## Hỗ trợ

Nếu gặp vấn đề, kiểm tra:
1. Logs của containers
2. Windows Event Viewer
3. Docker Desktop logs
4. Network connectivity

