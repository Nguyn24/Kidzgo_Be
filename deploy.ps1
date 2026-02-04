# Script tự động deploy Kidzgo Backend lên VPS
# Chạy script này với quyền Administrator

param(
    [switch]$Build = $false,
    [switch]$Stop = $false,
    [switch]$Start = $false,
    [switch]$Restart = $false,
    [switch]$Logs = $false,
    [switch]$Status = $false,
    [switch]$Backup = $false
)

Write-Host "=== Kidzgo Deployment Script ===" -ForegroundColor Cyan

# Kiểm tra Docker có đang chạy không
try {
    docker --version | Out-Null
    Write-Host "✓ Docker đã được cài đặt" -ForegroundColor Green
} catch {
    Write-Host "✗ Docker chưa được cài đặt hoặc chưa chạy!" -ForegroundColor Red
    Write-Host "Vui lòng cài đặt Docker Desktop và thử lại." -ForegroundColor Yellow
    exit 1
}

# Kiểm tra docker-compose
try {
    docker-compose --version | Out-Null
    Write-Host "✓ Docker Compose đã sẵn sàng" -ForegroundColor Green
} catch {
    Write-Host "✗ Docker Compose không khả dụng!" -ForegroundColor Red
    exit 1
}

# Kiểm tra file compose.yaml
if (-not (Test-Path "compose.yaml")) {
    Write-Host "✗ Không tìm thấy file compose.yaml!" -ForegroundColor Red
    Write-Host "Vui lòng chạy script này từ thư mục gốc của project." -ForegroundColor Yellow
    exit 1
}

# Xử lý các lệnh
if ($Stop) {
    Write-Host "`nĐang dừng tất cả containers..." -ForegroundColor Yellow
    docker-compose stop
    Write-Host "✓ Đã dừng containers" -ForegroundColor Green
    exit 0
}

if ($Start) {
    Write-Host "`nĐang khởi động containers..." -ForegroundColor Yellow
    docker-compose start
    Write-Host "✓ Đã khởi động containers" -ForegroundColor Green
    exit 0
}

if ($Restart) {
    Write-Host "`nĐang khởi động lại containers..." -ForegroundColor Yellow
    docker-compose restart
    Write-Host "✓ Đã khởi động lại containers" -ForegroundColor Green
    exit 0
}

if ($Status) {
    Write-Host "`nTrạng thái containers:" -ForegroundColor Cyan
    docker-compose ps
    exit 0
}

if ($Logs) {
    Write-Host "`nĐang hiển thị logs (Ctrl+C để dừng)..." -ForegroundColor Yellow
    docker-compose logs -f
    exit 0
}

if ($Backup) {
    Write-Host "`nĐang backup database..." -ForegroundColor Yellow
    $backupFile = "backup_$(Get-Date -Format 'yyyyMMdd_HHmmss').sql"
    docker-compose exec -T postgres pg_dump -U postgres kidzgo > $backupFile
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Backup thành công: $backupFile" -ForegroundColor Green
    } else {
        Write-Host "✗ Backup thất bại!" -ForegroundColor Red
    }
    exit 0
}

if ($Build) {
    Write-Host "`nĐang build và khởi động containers..." -ForegroundColor Yellow
    Write-Host "Quá trình này có thể mất vài phút..." -ForegroundColor Yellow
    
    docker-compose up -d --build
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`n✓ Build và deploy thành công!" -ForegroundColor Green
        Write-Host "`nĐang chờ containers khởi động..." -ForegroundColor Yellow
        Start-Sleep -Seconds 10
        
        Write-Host "`nTrạng thái containers:" -ForegroundColor Cyan
        docker-compose ps
        
        Write-Host "`nKiểm tra logs:" -ForegroundColor Cyan
        Write-Host "  docker-compose logs -f kidzgo.api" -ForegroundColor Gray
        Write-Host "  docker-compose logs -f postgres" -ForegroundColor Gray
        
        Write-Host "`nURLs:" -ForegroundColor Cyan
        Write-Host "  API: http://localhost/health" -ForegroundColor Gray
        Write-Host "  Swagger: http://localhost/swagger" -ForegroundColor Gray
        Write-Host "  Seq Logs: http://localhost:8081" -ForegroundColor Gray
    } else {
        Write-Host "`n✗ Build thất bại! Kiểm tra logs để biết thêm chi tiết." -ForegroundColor Red
        Write-Host "Chạy: docker-compose logs" -ForegroundColor Yellow
        exit 1
    }
} else {
    Write-Host "`nCách sử dụng:" -ForegroundColor Cyan
    Write-Host "  .\deploy.ps1 -Build      # Build và deploy lần đầu" -ForegroundColor Gray
    Write-Host "  .\deploy.ps1 -Restart    # Khởi động lại containers" -ForegroundColor Gray
    Write-Host "  .\deploy.ps1 -Stop       # Dừng containers" -ForegroundColor Gray
    Write-Host "  .\deploy.ps1 -Start      # Khởi động containers" -ForegroundColor Gray
    Write-Host "  .\deploy.ps1 -Status     # Xem trạng thái" -ForegroundColor Gray
    Write-Host "  .\deploy.ps1 -Logs       # Xem logs" -ForegroundColor Gray
    Write-Host "  .\deploy.ps1 -Backup     # Backup database" -ForegroundColor Gray
}

