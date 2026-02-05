Param(
    [string]$Branch = "main",
    # Đường dẫn mặc định tới source code trên VPS
    [string]$ProjectPath = "C:\Users\Administrator\Desktop\Projects\Kidzgo",
    [string]$PublishPath = "C:\apps\kidzgo-api",
    [string]$ServiceName = "KidzgoAPI"
)

Write-Host "==== Kidzgo deploy script (Windows) ====" -ForegroundColor Cyan

function Stop-ServiceSafe {
    param([string]$Name)
    try {
        $svc = Get-Service -Name $Name -ErrorAction Stop
        if ($svc.Status -eq 'Running') {
            Write-Host "Stopping service $Name..." -ForegroundColor Yellow
            Stop-Service -Name $Name -Force -ErrorAction Stop
            $svc.WaitForStatus('Stopped','00:00:20')
        }
    }
    catch {
        Write-Host "Service $Name not found or cannot be stopped (may be first deploy)." -ForegroundColor DarkYellow
    }
}

function Start-ServiceSafe {
    param([string]$Name)
    try {
        Write-Host "Starting service $Name..." -ForegroundColor Yellow
        Start-Service -Name $Name -ErrorAction Stop
        $svc = Get-Service -Name $Name
        $svc.WaitForStatus('Running','00:00:20')
        Write-Host "Service $Name is running." -ForegroundColor Green
    }
    catch {
        Write-Host "ERROR: Could not start service $Name" -ForegroundColor Red
        throw
    }
}

Write-Host "`nStep 1/4: Go to project directory" -ForegroundColor Cyan
Set-Location $ProjectPath

Write-Host "`nStep 2/4: Pull latest code from branch '$Branch'" -ForegroundColor Cyan
git fetch origin
git checkout $Branch
git pull origin $Branch

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: git pull failed, aborting deploy." -ForegroundColor Red
    exit 1
}

Write-Host "`nStep 3/4: Stop service and publish Kidzgo.API (Release) to $PublishPath" -ForegroundColor Cyan

# QUAN TRỌNG: Stop service TRƯỚC khi publish để tránh file lock
Stop-ServiceSafe -Name $ServiceName

# Đợi thêm 2 giây để đảm bảo process đã release file lock
Write-Host "Waiting 2 seconds for file locks to be released..." -ForegroundColor Yellow
Start-Sleep -Seconds 2

# Publish vào thư mục tạm trước
$TempPublishPath = "$PublishPath-temp"
if (Test-Path $TempPublishPath) {
    Remove-Item -Path $TempPublishPath -Recurse -Force -ErrorAction SilentlyContinue
}

if (-not (Test-Path $TempPublishPath)) {
    New-Item -ItemType Directory -Path $TempPublishPath | Out-Null
}

Write-Host "Publishing to temporary directory: $TempPublishPath" -ForegroundColor Yellow
dotnet publish ".\Kidzgo.API\Kidzgo.API.csproj" -c Release -o $TempPublishPath

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: dotnet publish failed, aborting deploy." -ForegroundColor Red
    # Start lại service nếu publish fail
    Start-ServiceSafe -Name $ServiceName
    exit 1
}

# Xóa thư mục cũ và copy thư mục mới
Write-Host "Replacing old files with new ones..." -ForegroundColor Yellow
if (Test-Path $PublishPath) {
    Remove-Item -Path $PublishPath -Recurse -Force -ErrorAction SilentlyContinue
}
Move-Item -Path $TempPublishPath -Destination $PublishPath -Force

Write-Host "`nStep 4/4: Start Windows service '$ServiceName'" -ForegroundColor Cyan

Start-ServiceSafe -Name $ServiceName

Write-Host "`n✅ Deploy completed successfully." -ForegroundColor Green
Write-Host "API should now be available at configured URL/port on the VPS." -ForegroundColor Green


