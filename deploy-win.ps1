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

Write-Host "`nStep 3/4: Publish Kidzgo.API (Release) to $PublishPath" -ForegroundColor Cyan

if (-not (Test-Path $PublishPath)) {
    New-Item -ItemType Directory -Path $PublishPath | Out-Null
}

dotnet publish ".\Kidzgo.API\Kidzgo.API.csproj" -c Release -o $PublishPath

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: dotnet publish failed, aborting deploy." -ForegroundColor Red
    exit 1
}

Write-Host "`nStep 4/4: Restart Windows service '$ServiceName'" -ForegroundColor Cyan

Stop-ServiceSafe -Name $ServiceName
Start-ServiceSafe -Name $ServiceName

Write-Host "`n✅ Deploy completed successfully." -ForegroundColor Green
Write-Host "API should now be available at configured URL/port on the VPS." -ForegroundColor Green


