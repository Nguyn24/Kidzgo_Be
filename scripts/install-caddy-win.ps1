Param(
    [string]$Version = "2.10.2",
    [string]$InstallRoot = "C:\\Caddy",
    [string]$Domain = "rexengswagger.duckdns.org",
    [string]$ApiUpstream = "127.0.0.1:5000",
    [string]$Email = ""
)

$ErrorActionPreference = "Stop"

Write-Host "Installing Caddy $Version to $InstallRoot" -ForegroundColor Cyan

New-Item -ItemType Directory -Force -Path $InstallRoot | Out-Null

$zipPath = Join-Path $env:TEMP "caddy_${Version}_windows_amd64.zip"
$downloadUrl = "https://github.com/caddyserver/caddy/releases/download/v$Version/caddy_${Version}_windows_amd64.zip"

Invoke-WebRequest -Uri $downloadUrl -OutFile $zipPath
Expand-Archive -Path $zipPath -DestinationPath $InstallRoot -Force

$caddyFilePath = Join-Path $InstallRoot "Caddyfile"
$caddyFile = @"
$Domain {
    encode zstd gzip

    header {
        Strict-Transport-Security "max-age=31536000; includeSubDomains; preload"
        X-Content-Type-Options "nosniff"
        X-Frame-Options "SAMEORIGIN"
        Referrer-Policy "strict-origin-when-cross-origin"
    }

    reverse_proxy $ApiUpstream
}
"@

Set-Content -Path $caddyFilePath -Value $caddyFile -Encoding ASCII

Push-Location $InstallRoot

if (-not [string]::IsNullOrWhiteSpace($Email)) {
    & .\caddy.exe trust
    & .\caddy.exe validate --config $caddyFilePath
    & .\caddy.exe service uninstall 2>$null
    & .\caddy.exe service install --config $caddyFilePath --adapter caddyfile
}
else {
    & .\caddy.exe trust
    & .\caddy.exe validate --config $caddyFilePath
    & .\caddy.exe service uninstall 2>$null
    & .\caddy.exe service install --config $caddyFilePath --adapter caddyfile
}

Start-Service caddy
Get-Service caddy

Pop-Location

Write-Host "Caddy installed. Test URL: https://$Domain/swagger/index.html" -ForegroundColor Green
